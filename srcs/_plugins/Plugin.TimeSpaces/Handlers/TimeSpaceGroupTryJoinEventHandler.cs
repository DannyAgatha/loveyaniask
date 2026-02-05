using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Portals;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceGroupTryJoinEventHandler : IAsyncEventProcessor<TimeSpaceGroupTryJoinEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;
    private readonly ISubActConfiguration _subActConfiguration;
    private readonly ITimeSpaceConfiguration _timeSpaceConfiguration;

    public TimeSpaceGroupTryJoinEventHandler(IItemsManager itemsManager, IGameLanguageService gameLanguage, ISubActConfiguration subActConfiguration, ITimeSpaceConfiguration timeSpaceConfiguration)
    {
        _itemsManager = itemsManager;
        _gameLanguage = gameLanguage;
        _subActConfiguration = subActConfiguration;
        _timeSpaceConfiguration = timeSpaceConfiguration;
    }

    public async Task HandleAsync(TimeSpaceGroupTryJoinEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        ITimeSpacePortalEntity portal = e.PortalEntity;
        long characterId = e.CharacterId;

        if (session.PlayerEntity.Id == characterId)
        {
            return;
        }

        if (!session.PlayerEntity.IsInGroup())
        {
            return;
        }

        if (session.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        if (session.IsMuted())
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.MUTE_SHOUTMESSAGE_YOU_ARE_MUTED), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        IPlayerEntity groupMember = session.PlayerEntity.GetGroup().Members.FirstOrDefault(x => x.Id == characterId);
        if (groupMember == null)
        {
            return;
        }

        if (!groupMember.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        TimeSpaceParty timeSpace = groupMember.TimeSpaceComponent.TimeSpace;

        if (timeSpace.Instance == null)
        {
            return;
        }

        if (timeSpace.TimeSpaceId != portal.TimeSpaceId)
        {
            return;
        }

        if (groupMember.TimeSpaceComponent.TimeSpaceTeamIsFull)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_TS_FULL), MsgMessageType.Middle);
            return;
        }

        if (timeSpace.Started || timeSpace.Finished)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_ALREADY_STARTED), MsgMessageType.Middle);
            return;
        }
        if (session.PlayerEntity.Level < timeSpace.TimeSpaceInformation.MinLevel)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_LOW_LEVEL), ChatMessageColorType.PlayerSay);
            return;
        }
        
        if (session.PlayerEntity.Level > timeSpace.TimeSpaceInformation.MaxLevel)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_HIGH_LEVEL), ChatMessageColorType.PlayerSay);
            return;
        }

        if (!session.CanJoinToTimeSpace(timeSpace.TimeSpaceId, _subActConfiguration, _timeSpaceConfiguration) && !session.IsGameMaster()
            && !timeSpace.TimeSpaceInformation.IsSpecial && !timeSpace.TimeSpaceInformation.IsHidden)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_WRONG_ACT), MsgMessageType.Middle);
            return;
        }

        if (timeSpace.TimeSpaceInformation.SeedsOfPowerRequired != 0)
        {
            if (!session.PlayerEntity.HasItem((short)ItemVnums.SEED_OF_POWER, timeSpace.TimeSpaceInformation.SeedsOfPowerRequired))
            {
                string itemName = _itemsManager.GetItem((short)ItemVnums.SEED_OF_POWER).GetItemName(_gameLanguage, session.UserLanguage);
                session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS,
                    timeSpace.TimeSpaceInformation.SeedsOfPowerRequired, itemName), ChatMessageColorType.PlayerSay);
                return;
            }

            await session.RemoveItemFromInventory((short)ItemVnums.SEED_OF_POWER, timeSpace.TimeSpaceInformation.SeedsOfPowerRequired);
        }

        session.PlayerEntity.TimeSpaceComponent.SetTimeSpaceParty(timeSpace);
        timeSpace.AddMember(session);

        IMapInstance mapStart = timeSpace.Instance.SpawnInstance.MapInstance;

        session.ChangeMap(mapStart, timeSpace.Instance.SpawnPoint.X, timeSpace.Instance.SpawnPoint.Y);
        session.SendPacket(mapStart.GenerateRsfn(true, false));
        foreach (KeyValuePair<Guid, TimeSpaceSubInstance> mapInstance in timeSpace.Instance.TimeSpaceSubInstances)
        {
            session.SendPacket(mapInstance.Value.MapInstance.GenerateRsfn(isVisit: false));
        }

        session.SendRsfpPacket();
        mapStart.MapClear(true);
        mapStart.BroadcastTimeSpacePartnerInfo();
        session.SendRsfmPacket(TimeSpaceAction.CAMERA_ADJUST);
        session.SendMinfo();
        if (timeSpace.IsEasyMode)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_EASY_MODE), ChatMessageColorType.Yellow);
        }

        if (session.PlayerEntity.Level > timeSpace.HigherLevel)
        {
            timeSpace.HigherLevel = session.PlayerEntity.Level;
        }
    }
}