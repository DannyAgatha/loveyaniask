using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Scheduler;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Portals;
using WingsEmu.Game.PrivateMapInstances;
using WingsEmu.Game.PrivateMapInstances.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.PrivateMapInstance;

public class CreatePrivateMapInstanceEventHandler : IAsyncEventProcessor<CreatePrivateMapInstanceEvent>
{
    private readonly IPrivateMapInstanceManager _privateMapInstanceManager;
    private readonly IMapManager _mapManager;
    private readonly IPortalFactory _portalFactory;
    private readonly IScheduler _scheduler;
    private readonly PrivateInstanceRatesConfiguration _privateInstanceRatesConfiguration;
    private readonly IGameLanguageService _gameLanguageService;

    public CreatePrivateMapInstanceEventHandler(IPrivateMapInstanceManager privateMapInstanceManager, IMapManager mapManager, IPortalFactory portalFactory, IScheduler scheduler,
        PrivateInstanceRatesConfiguration privateInstanceRatesConfiguration, IGameLanguageService gameLanguageService)
    {
        _privateMapInstanceManager = privateMapInstanceManager;
        _mapManager = mapManager;
        _portalFactory = portalFactory;
        _scheduler = scheduler;
        _privateInstanceRatesConfiguration = privateInstanceRatesConfiguration;
        _gameLanguageService = gameLanguageService;
    }

    public async Task HandleAsync(CreatePrivateMapInstanceEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IPlayerEntity playerEntity = session.PlayerEntity;
        PrivateMapInstanceType type = e.Type;

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP), MsgMessageType.Middle);
            return;
        }

        if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP), MsgMessageType.Middle);
            return;
        }

        bool haveGroupOrFamily = type switch
        {
            PrivateMapInstanceType.GROUP => playerEntity.IsInGroup(),
            PrivateMapInstanceType.FAMILY => playerEntity.IsInFamily(),
            _ => true
        };

        if (!haveGroupOrFamily)
        {
            session.SendMsg(
                session.GetLanguage(type == PrivateMapInstanceType.GROUP ? GameDialogKey.PRIVATE_MAP_INSTANCE_MESSAGE_YOU_NEED_GROUP : GameDialogKey.PRIVATE_MAP_INSTANCE_MESSAGE_YOU_NEED_FAMILY),
                MsgMessageType.Middle);
            return;
        }

        WingsEmu.Game.PrivateMapInstances.PrivateMapInstance currentPrivateMapInstance = type switch
        {
            PrivateMapInstanceType.SOLO => _privateMapInstanceManager.GetByPlayerId(playerEntity.Id),
            PrivateMapInstanceType.GROUP => _privateMapInstanceManager.GetByGroupId(playerEntity.GetGroup()?.GroupId ?? -1),
            PrivateMapInstanceType.FAMILY => _privateMapInstanceManager.GetByFamilyId(playerEntity.Family?.Id ?? -1),
            PrivateMapInstanceType.PUBLIC => _privateMapInstanceManager.GetByMapVnum(playerEntity.MapInstance.MapVnum),
            _ => null
        };

        if (currentPrivateMapInstance != null)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.PRIVATE_MAP_INSTANCE_MESSAGE_ALREADY_EXISTS), MsgMessageType.Middle);
            return;
        }

        if (type == PrivateMapInstanceType.PUBLIC)
        {
            if (e.Sender.CurrentMapInstance.Portals.Any(por => Math.Abs(e.Sender.PlayerEntity.PositionX - por.PositionX) < 6 && Math.Abs(e.Sender.PlayerEntity.PositionY - por.PositionY) < 6))
            {
                session.SendMsg(session.GetLanguage(GameDialogKey.PRIVATE_MAP_INSTANCE_MESSAGE_NEAR_PORTAL), MsgMessageType.Middle);
                return;
            }

            if (e.Sender.CurrentMapInstance.TimeSpacePortals.Any(por =>
                    Math.Abs(e.Sender.PlayerEntity.PositionX - por.Position.X) < 6 && Math.Abs(e.Sender.PlayerEntity.PositionY - por.Position.Y) < 6))
            {
                session.SendMsg(session.GetLanguage(GameDialogKey.PRIVATE_MAP_INSTANCE_MESSAGE_NEAR_PORTAL), MsgMessageType.Middle);
                return;
            }
        }

        IMapInstance mapInstance = _mapManager.GeneratePrivateMapInstance(session.CurrentMapInstance.MapVnum);
        if (mapInstance == null)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.PRIVATE_MAP_INSTANCE_MESSAGE_COULD_NOT_CREATE_MAP), MsgMessageType.Middle);
            return;
        }

        IMapInstance currentMap = _mapManager.GetMapInstance(playerEntity.MapInstance.Id);
        IPortalEntity exitPortal = _portalFactory.CreatePortal(PortalType.TSNormal, currentMap, playerEntity.Position);
        mapInstance.AddPortalToMap(exitPortal, sendGpPacket: false);

        if (type == PrivateMapInstanceType.PUBLIC)
        {
            IPortalEntity enterPortal = _portalFactory.CreatePortal(PortalType.TSNormal, currentMap, playerEntity.Position, mapInstance, playerEntity.Position);
            currentMap.AddPortalToMap(enterPortal, scheduler: _scheduler, timeInSeconds: (int)TimeSpan.FromMinutes(59).TotalSeconds, isTemporary: true);
        }

        var privateMapInstance = new WingsEmu.Game.PrivateMapInstances.PrivateMapInstance
        {
            MapInstance = mapInstance,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow + TimeSpan.FromHours(1),
            Type = type,
            IsPremium = e.IsPremium,
            IsHardcore = e.IsHardcore,
            PlayerId = type is PrivateMapInstanceType.SOLO ? playerEntity.Id : null,
            GroupId = type is PrivateMapInstanceType.GROUP ? playerEntity.GetGroup().GroupId : null,
            FamilyId = type is PrivateMapInstanceType.FAMILY ? playerEntity.Family!.Id : null,
            MapVnum = type is PrivateMapInstanceType.PUBLIC ? currentMap.MapVnum : null
        };

        switch (type)
        {
            case PrivateMapInstanceType.SOLO:

                session.SendChatMessage(session.GetLanguage(GetJoinMessageKey(privateMapInstance)), ChatMessageColorType.Yellow);
                session.ChangeMap(mapInstance, playerEntity.Position.X, playerEntity.Position.Y);
                session.SendTsClockPacket(privateMapInstance.EndTime - DateTime.UtcNow, true);
                session.PlayerEntity.PrivateMapInstanceInfo = new PrivateMapInstanceInfo();
                _privateMapInstanceManager.AddByPlayer(playerEntity.Id, privateMapInstance);

                if(privateMapInstance.IsPremium || privateMapInstance.IsHardcore)
                {
                    PrivateInstanceRates rates = _privateInstanceRatesConfiguration.GetRates(
                        type,
                        isPremium: privateMapInstance.IsPremium,
                        isHardcore: privateMapInstance.IsHardcore
                    );

                    string formattedRates = _gameLanguageService.GetLanguageFormat(
                        GameDialogKey.PRIVATE_MAP_INSTANCE_RATES_INFO,
                        session.UserLanguage,
                        (rates.DropItem * 100).ToString("0.#"),
                        (rates.DropGold * 100).ToString("0.#"),
                        (rates.ExpCharacter * 100).ToString("0.#"),
                        (rates.ExpHero * 100).ToString("0.#"),
                        (rates.ExpFairy * 100).ToString("0.#"),
                        (rates.ExpJob * 100).ToString("0.#"),
                        (rates.ExpSp * 100).ToString("0.#"),
                        (rates.ExpMate * 100).ToString("0.#"),
                        rates.MonsterAdditionalSpeed.ToString(),
                        (rates.MonsterRespawnReducer * 100).ToString("0.#"),
                        rates.InstanceCostGold.ToString("N0")
                    );

                    session.SendInfo(formattedRates);
                    session.SendChatMessage(formattedRates, ChatMessageColorType.Yellow);
                }

                break;

            case PrivateMapInstanceType.GROUP:
                PlayerGroup group = playerEntity.GetGroup();

                foreach (IClientSession mapSession in playerEntity.MapInstance.Sessions.ToArray())
                {
                    if (mapSession.PlayerEntity.GetGroup()?.GroupId != group.GroupId)
                    {
                        continue;
                    }

                    mapSession.SendChatMessage(mapSession.GetLanguage(GetJoinMessageKey(privateMapInstance)), ChatMessageColorType.Yellow);
                    mapSession.ChangeMap(mapInstance, playerEntity.Position.X, playerEntity.Position.Y);
                    mapSession.SendTsClockPacket(privateMapInstance.EndTime - DateTime.UtcNow, true);
                    mapSession.PlayerEntity.PrivateMapInstanceInfo = new PrivateMapInstanceInfo();
                }

                _privateMapInstanceManager.AddByGroup(group.GroupId, privateMapInstance);

                if (privateMapInstance.IsPremium || privateMapInstance.IsHardcore)
                {
                    PrivateInstanceRates groupRates = _privateInstanceRatesConfiguration.GetRates(
                        type,
                        isPremium: privateMapInstance.IsPremium,
                        isHardcore: privateMapInstance.IsHardcore
                    );

                    foreach (IClientSession mapSession in playerEntity.MapInstance.Sessions.ToArray())
                    {
                        if (mapSession.PlayerEntity.GetGroup()?.GroupId == group.GroupId)
                        {
                            string formattedRates = _gameLanguageService.GetLanguageFormat(
                                GameDialogKey.PRIVATE_MAP_INSTANCE_RATES_INFO,
                                mapSession.UserLanguage,
                                (groupRates.DropItem * 100).ToString("0.#"),
                                (groupRates.DropGold * 100).ToString("0.#"),
                                (groupRates.ExpCharacter * 100).ToString("0.#"),
                                (groupRates.ExpHero * 100).ToString("0.#"),
                                (groupRates.ExpFairy * 100).ToString("0.#"),
                                (groupRates.ExpJob * 100).ToString("0.#"),
                                (groupRates.ExpSp * 100).ToString("0.#"),
                                (groupRates.ExpMate * 100).ToString("0.#"),
                                groupRates.MonsterAdditionalSpeed.ToString(),
                                (groupRates.MonsterRespawnReducer * 100).ToString("0.#"),
                                groupRates.InstanceCostGold.ToString("N0")
                            );

                            mapSession.SendInfo(formattedRates);
                            mapSession.SendChatMessage(
                                mapSession.GetLanguage(GetJoinMessageKey(privateMapInstance)),
                                ChatMessageColorType.Yellow
                            );
                        }
                    }
                }

                break;

            case PrivateMapInstanceType.FAMILY:

                foreach (IClientSession mapSession in playerEntity.MapInstance.Sessions.ToArray())
                {
                    if (mapSession.PlayerEntity.Family is null || mapSession.PlayerEntity.Family.Id != playerEntity.Family!.Id)
                    {
                        continue;
                    }

                    mapSession.SendChatMessage(mapSession.GetLanguage(GetJoinMessageKey(privateMapInstance)), ChatMessageColorType.Yellow);
                    mapSession.ChangeMap(mapInstance, playerEntity.Position.X, playerEntity.Position.Y);
                    mapSession.SendTsClockPacket(privateMapInstance.EndTime - DateTime.UtcNow, true);
                    mapSession.PlayerEntity.PrivateMapInstanceInfo = new PrivateMapInstanceInfo();
                }

                _privateMapInstanceManager.AddByFamily(playerEntity.Family!.Id, privateMapInstance);

                break;
            case PrivateMapInstanceType.PUBLIC:
                _privateMapInstanceManager.AddByMapVnum(currentMap.MapVnum, privateMapInstance);
                break;
        }
    }

    private GameDialogKey GetJoinMessageKey(WingsEmu.Game.PrivateMapInstances.PrivateMapInstance privateMapInstance)
    {
        if (privateMapInstance.IsHardcore)
        {
            return GameDialogKey.PRIVATE_MAP_INSTANCE_CHATMESSAGE_JOINED_TO_HARDCORE_MAP;
        }

        return privateMapInstance.IsPremium ? GameDialogKey.PRIVATE_MAP_INSTANCE_CHATMESSAGE_JOINED_TO_PREMIUM_MAP : GameDialogKey.PRIVATE_MAP_INSTANCE_CHATMESSAGE_JOINED_TO_MAP;
    }
}