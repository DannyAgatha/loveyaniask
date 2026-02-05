using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Packets.Enums.LandOfLife;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Act6.Event;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.WorldBoss;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Families;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Portals;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Maps;

public class JoinMapEventHandler : IAsyncEventProcessor<JoinMapEvent>
{
    private readonly IBuffFactory _buffFactory;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameLanguageService _languageService;
    private readonly IMapManager _mapManager;
    private readonly IMinilandManager _minilandManager;
    private readonly IPortalFactory _portalFactory;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly SerializableGameServer _serializableGameServer;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;
    private readonly ISessionManager _sessionManager;
    private readonly IAsyncEventPipeline _eventPipeline;

    public JoinMapEventHandler(ICharacterAlgorithm characterAlgorithm, IGameLanguageService languageService, ISpPartnerConfiguration spPartnerConfiguration,
        IBuffFactory buffFactory, IMinilandManager minilandManager, IMapManager mapManager,
        IReputationConfiguration reputationConfiguration, IPortalFactory portalFactory, IRankingManager rankingManager,
        SerializableGameServer serializableGameServer, ISessionManager sessionManager, IAsyncEventPipeline eventPipeline)
    {
        _characterAlgorithm = characterAlgorithm;
        _languageService = languageService;
        _spPartnerConfiguration = spPartnerConfiguration;
        _buffFactory = buffFactory;
        _minilandManager = minilandManager;
        _mapManager = mapManager;
        _reputationConfiguration = reputationConfiguration;
        _portalFactory = portalFactory;
        _rankingManager = rankingManager;
        _serializableGameServer = serializableGameServer;
        _sessionManager = sessionManager;
        _eventPipeline = eventPipeline;
    }

    public async Task HandleAsync(JoinMapEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IMapInstance mapInstance;

        if (e.JoinedMapInstance != default)
        {
            mapInstance = e.JoinedMapInstance;
        }
        else if (e.JoinedMapGuid != default)
        {
            mapInstance = _mapManager.GetMapInstance(e.JoinedMapGuid);
        }
        else
        {
            mapInstance = _mapManager.GetBaseMapInstanceByMapId(e.JoinedMapId);
        }

        if (mapInstance == default)
        {
            /*if (_serializableGameServer.ChannelType == GameChannelType.ACT_4 && e.JoinedMapId is 145)
            {
                await session.EmitEventAsync(new PlayerReturnFromAct4Event());
                return;
            }*/

            Log.Error(
                "Seems like the provided information for the Map to join doesn't equal to any known instance." +
                $"JoinedMapInstance: {e.JoinedMapInstance?.Id}|{e.JoinedMapInstance?.MapId}" +
                $"JoinedMapGuid: {e.JoinedMapGuid}" +
                $"JoinedMapId: {e.JoinedMapId}" +
                $"ChannelType: {_serializableGameServer.ChannelType} | ChannelId: {_serializableGameServer.ChannelId}", new ArgumentNullException("MapInstance"));
            session.ChangeToLastBaseMap();
            return;
        }

        Stopwatch watch = null;

        if (session.DebugMode)
        {
            watch = Stopwatch.StartNew();
        }

        if (session.CurrentMapInstance != default)
        {
            await session.EmitEventAsync(new LeaveMapEvent());
        }

        ServerSideJoinMap(e, mapInstance);
        IFamily family = session.PlayerEntity.Family;

        await SendJoinMapInformation(e, family);
        await BroadcastAndReceiveJoinInformation(e, family);
        await session.EmitEventAsync(new JoinMapEndEvent(mapInstance));

        await session.PlayerEntity.CheckAct52Buff(_buffFactory);
        await session.PlayerEntity.CheckAct4Buff(_buffFactory);
        await session.PlayerEntity.CheckPvPBuff();
        await session.EmitEventAsync(new VehicleCheckMapSpeedEvent());
        session.PlayerEntity.ShadowAppears(false);

        session.PlayerEntity.LastMapChange = DateTime.UtcNow;
        if (session.DebugMode && watch != null)
        {
            watch.Stop();
            session.SendDebugMessage($"The map change took: {watch.ElapsedMilliseconds.ToString()}ms");
        }

        if (session.PlayerEntity.IsRaidLeader(session.PlayerEntity.Id))
        {
            if (session.PlayerEntity.Raid?.Instance?.RaidSubInstances == null)
            {
                return;
            }

            if (session.CurrentMapInstance != null && session.PlayerEntity.Raid.Instance.RaidSubInstances.TryGetValue(session.CurrentMapInstance.Id, out RaidSubInstance subInstance) && subInstance != null)
            {
                subInstance.IsDiscoveredByLeader = true;
                
                foreach (IClientSession member in session.PlayerEntity.Raid.Members)
                {
                    member.PlayerEntity?.HardcoreComponent?.ResetDamage();
                }
            }
        }
    }

    private void ServerSideJoinMap(JoinMapEvent e, IMapInstance mapInstance)
    {
        IClientSession session = e.Sender;

        if (mapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) && mapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            session.PlayerEntity.MapId = mapInstance.MapId;
        }

        if (mapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) && mapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance && e.X != null && e.Y != null)
        {
            session.PlayerEntity.MapX = e.X.Value;
            session.PlayerEntity.MapY = e.Y.Value;
        }

        if (e.X != null && e.Y != null)
        {
            session.PlayerEntity.Position = new Position(e.X.Value, e.Y.Value);
        }

        switch (mapInstance.MapInstanceType)
        {
            case MapInstanceType.Miniland:
                WingsEmu.Game.Configurations.Miniland.Miniland minilandConfiguration = _minilandManager.GetMinilandConfiguration(mapInstance);
                session.PlayerEntity.Position = new Position(minilandConfiguration.ArrivalSerializablePosition.X, minilandConfiguration.ArrivalSerializablePosition.Y);

                if (mapInstance.Id == session.PlayerEntity.Miniland.Id)
                {
                    foreach (IMateEntity mate in session.PlayerEntity.MateComponent.GetMates(x => !x.IsTeamMember))
                    {
                        session.SendCondMate(mate);
                        if (mapInstance.GetMateById(mate.Id) != null)
                        {
                            continue;
                        }

                        mapInstance.AddMate(mate);
                    }
                    
                    session.SendScnPackets();
                    session.SendScpPackets();
                }

                break;
            case MapInstanceType.ArenaInstance:
                session.PlayerEntity.ArenaImmunity = DateTime.UtcNow;
                session.SendArenaStatistics(false, session.PlayerEntity.GetGroup());
                break;
            case MapInstanceType.TimeSpaceInstance:

                List<INpcEntity> partners = session.PlayerEntity.TimeSpaceComponent.Partners;
                if (!partners.Any())
                {
                    break;
                }

                foreach (INpcEntity partner in partners)
                {
                    partner.ChangeMapInstance(mapInstance);
                    mapInstance.AddNpc(partner);
                    partner.Position = session.PlayerEntity.Position;
                }

                break;

            default:
                session.PlayerEntity.ArenaDeaths = 0;
                session.PlayerEntity.ArenaKills = 0;
                session.PlayerEntity.TimeSpaceComponent.Partners.Clear();
                break;
        }

        session.PlayerEntity.MapInstanceId = mapInstance.Id;
        session.CurrentMapInstance = session.PlayerEntity.MapInstance;

        session.CurrentMapInstance.RegisterSession(session);
        session.SendDiscordRpcPacket();
        
        HashSet<Guid> toRemovePlayer = [];
        foreach (Guid id in session.PlayerEntity.AggroedEntities)
        {
            IMonsterEntity monster = mapInstance.GetMonsterByUniqueId(id);
            if (monster != null)
            {
                continue;
            }

            toRemovePlayer.Add(id);
        }

        foreach (Guid remove in toRemovePlayer)
        {
            session.PlayerEntity.AggroedEntities.Remove(remove);
        }


        foreach (IMateEntity mate in session.PlayerEntity.MateComponent.TeamMembers())
        {
            HashSet<Guid> toRemoveMate = new();
            foreach (Guid id in mate.AggroedEntities)
            {
                IMonsterEntity monster = mapInstance.GetMonsterByUniqueId(id);
                if (monster != null)
                {
                    continue;
                }

                toRemoveMate.Add(id);
            }

            foreach (Guid remove in toRemoveMate)
            {
                mate.AggroedEntities.Remove(remove);
            }
        }
    }

    private async Task SendJoinMapInformation(JoinMapEvent e, IFamily family)
    {
        IClientSession session = e.Sender;
        string mapName = _languageService.GetMapName(_mapManager.GetMapByMapId(session.PlayerEntity.MapId), session);

        session.SendCInfoPacket(family, _reputationConfiguration, _rankingManager.TopReputation);
        session.SendCModePacket();
        session.RefreshEquipment();
        session.RefreshLevel(_characterAlgorithm);
        session.RefreshStat();
        session.SendAtPacket();

        session.SendGidxPacket(family, _languageService);
        session.SendCondPacket();
        session.SendCondPacket();
        session.SendTitInfoPacket();
        session.SendEqPacket();
        session.SendCMapPacket(true);

        if (session.CurrentMapInstance.MapId == (int)MapIds.HATUS_BOSS_MAP)
        {
            session.SendEmptyHatusHeads();
        }

        session.RefreshStatChar();
        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            session.SendRsfpPacket();
        }

        session.RefreshFairy();
        session.RefreshMiniPet();
        session.SendCharConstBuffEffect();
        session.RefreshZoom();

        await SendPartyInfo(e);
        
        if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_6_1))
        {
            await session.EmitEventAsync(new Act6PacketGenerateEvent());          
        }
        else
        {
            session.SendPacket(UiPacketExtension.GenerateNullAct6Packet());
        }
        
        session.SendCondPacket();

        session.SendPacket(session.CurrentMapInstance.GenerateMapDesignObjects());
        foreach (MapDesignObject mapObject in session.CurrentMapInstance.MapDesignObjects)
        {
            session.SendPacket(mapObject.GenerateEffect(false));
        }

        if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance && session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            IClientSession[] timeSpaceMembers = session.PlayerEntity.TimeSpaceComponent.TimeSpace.Members.ToArray();
            foreach (IClientSession member in timeSpaceMembers)
            {
                member.SendPacket(e.JoinedMapInstance.GenerateRsfn(isVisit: true));
            }

            if (session.PlayerEntity.TimeSpaceComponent.TimeSpace.Started)
            {
                session.SendRsfpPacket();
            }
        }

        session.SendPackets(session.CurrentMapInstance.GetEntitiesOnMapPackets());

        foreach (IPortalEntity p in session.CurrentMapInstance.GenerateMinilandEntryPortals(session.PlayerEntity.Miniland, _portalFactory))
        {
            session.SendPacket(p.GenerateGp());
        }

        session.SendTimeSpacePortals();

        session.TrySendScalPacket();
        session.SendDancePacket(session.CurrentMapInstance.IsDance);

        if (session.PlayerEntity.ShowRaidDeathInfo)
        {
            session.PlayerEntity.ShowRaidDeathInfo = false;
            session.SendInfo(session.GetLanguage(GameDialogKey.RAID_INFO_NO_LIVES_LEFT));
        }

        List<INpcEntity> partners = session.PlayerEntity.TimeSpaceComponent.Partners;
        if (partners.Any())
        {
            foreach (INpcEntity partner in partners)
            {
                session.PlayerEntity.MapInstance.Broadcast(x => partner.GenerateIn());
                session.SendMateControl(partner);
                session.SendCondMate(partner);
            }
        }
        await RejoinPreviousTeam(session, e.LastRaid, e.IsMarathonMode);
        await MinilandInfoSend(e);
    }
    
    private async Task RejoinPreviousTeam(IClientSession session, RaidParty party, bool isMarathonMode)
    {
        if (!isMarathonMode)
        {
            return;
        }

        if (party.Leader == null)
        {
            return;
        }

        if (party.Leader.PlayerEntity.Id == session.PlayerEntity.Id)
        {
            if (session.PlayerEntity.IsInGroup())
            {
                return;
            }
            
            if (session.PlayerEntity.LastSeal == null)
            {
                return;
            }
            
            if (!session.PlayerEntity.HasItem(session.PlayerEntity.LastSeal.ItemInstance.ItemVNum))
            {
                return;
            }

            InventoryItem item = session.PlayerEntity.GetFirstItemByVnum(session.PlayerEntity.LastSeal.ItemInstance.ItemVNum);
            await session.EmitEventAsync(new RaidPartyCreateEvent((byte)party.Type, item, party.IsMarathonMode));
        }
        else
        {
            IClientSession leader = _sessionManager.GetSessionByCharacterId(party.Leader.PlayerEntity.Id);

            if (leader.PlayerEntity.LastSeal == null)
            {
                return;
            }

            if (!leader.PlayerEntity.HasItem(leader.PlayerEntity.LastSeal.ItemInstance.ItemVNum))
            {
                return;
            }
            
            await session.EmitEventAsync(new RaidPartyJoinEvent(party.Leader.PlayerEntity.Id, true, party.IsMarathonMode));
        }
    }

    private async Task MinilandInfoSend(PlayerEvent e)
    {
        if (e.Sender.CurrentMapInstance?.MapInstanceType != MapInstanceType.Miniland)
        {
            return;
        }

        IClientSession session = e.Sender;
        IClientSession minilandOwner = _minilandManager.GetSessionByMiniland(session.CurrentMapInstance);
        if (minilandOwner == default)
        {
            return;
        }

        if (session.PlayerEntity.Miniland.Id != minilandOwner.PlayerEntity.Miniland.Id)
        {
            if (minilandOwner.PlayerEntity.MinilandState == MinilandState.LOCK && !session.IsGameMaster()
                || minilandOwner.PlayerEntity.MinilandState == MinilandState.PRIVATE && !session.PlayerEntity.IsFriend(minilandOwner.PlayerEntity.Id)
                && !session.PlayerEntity.IsMarried(minilandOwner.PlayerEntity.Id) && !session.IsGameMaster())
            {
                session.ChangeToLastBaseMap();
                session.SendMsg(_languageService.GetLanguage(GameDialogKey.MINILAND_SHOUTMESSAGE_CLOSED, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            int count = minilandOwner.PlayerEntity.Miniland.Sessions.Count(x => x.PlayerEntity.Id != minilandOwner.PlayerEntity.Id && !x.GmMode);
            int capacity = _minilandManager.GetMinilandMaximumCapacity(minilandOwner.PlayerEntity.Id);

            if (count > capacity)
            {
                session.ChangeToLastBaseMap();
                session.SendMsg(_languageService.GetLanguage(GameDialogKey.MINILAND_SHOUTMESSAGE_FULL, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            session.SendMsg(minilandOwner.GetMinilandCleanMessage(_languageService), MsgMessageType.Middle);

            if (!session.IsGameMaster())
            {
                await _minilandManager.IncreaseMinilandVisitCounter(minilandOwner.PlayerEntity.Id);
            }

            session.SendMinilandPublicInformation(_minilandManager, _languageService);
            minilandOwner.SendMinilandPrivateInformation(_minilandManager, _languageService);
        }
        else
        {
            session.SendMinilandPrivateInformation(_minilandManager, _languageService);
        }

        foreach (IMateEntity mate in minilandOwner.PlayerEntity.MateComponent.GetMates())
        {
            if (!mate.IsAlive() && session.PlayerEntity.Id == minilandOwner.PlayerEntity.Id)
            {
                await session.EmitEventAsync(new MateReviveEvent(mate, false));
                continue;
            }

            if (!mate.IsTeamMember)
            {
                session.SendPacket(mate.GenerateIn(_languageService, session.UserLanguage, _spPartnerConfiguration));
            }
        }

        long visitCount = await _minilandManager.GetMinilandVisitCounter(session.PlayerEntity.Id);
        session.SendInformationChatMessage(_languageService.GetLanguageFormat(GameDialogKey.MINILAND_MESSAGE_VISITOR, session.UserLanguage,
            session.PlayerEntity.LifetimeStats.TotalMinilandVisits, visitCount));
    }

    private async Task SendPartyInfo(PlayerEvent e)
    {
        IClientSession session = e.Sender;

        session.SendPstPackets();
        if (!session.PlayerEntity.IsOnVehicle && !session.PlayerEntity.CheatComponent.IsInvisible)
        {
            foreach (IMateEntity mate in session.PlayerEntity.MateComponent.TeamMembers())
            {
                if (!mate.IsAlive())
                {
                    continue;
                }

                if (mate.IsSitting)
                {
                    await session.EmitEventAsync(new MateRestEvent
                    {
                        MateEntity = mate,
                        Force = true
                    });
                }

                mate.TeleportNearCharacter();
                session.SendPacket(mate.GenerateIn(_languageService, session.UserLanguage, _spPartnerConfiguration));
                session.TrySendScalPacket(mate);
                session.SendMateControl(mate);
                session.SendTargetConstBuffEffects(mate);
                session.SendCondMate(mate);
            }
        }

        session.RefreshParty(_spPartnerConfiguration);

        if (!session.PlayerEntity.IsInGroup())
        {
            return;
        }

        PlayerGroup group = session.PlayerEntity.GetGroup();

        foreach (IPlayerEntity member in group.Members)
        {
            if (session.PlayerEntity.Id == member.Id)
            {
                continue;
            }

            member.Session.RefreshParty(_spPartnerConfiguration);
        }

        session.CurrentMapInstance?.Broadcast(session.GeneratePidx(), new ExceptSessionBroadcast(session));
    }

    private async Task BroadcastAndReceiveJoinInformation(JoinMapEvent e, IFamily family)
    {
        IClientSession session = e.Sender;
        foreach (IClientSession currentPlayerOnMap in session.CurrentMapInstance.Sessions)
        {
            if (currentPlayerOnMap.PlayerEntity.Id == session.PlayerEntity.Id)
            {
                continue;
            }

            if (!currentPlayerOnMap.PlayerEntity.CheatComponent.IsInvisible)
            {
                await TargetedSendJoinInformation(session, currentPlayerOnMap, currentPlayerOnMap.PlayerEntity.Family, true);
            }

            if (!session.PlayerEntity.CheatComponent.IsInvisible)
            {
                await TargetedSendJoinInformation(currentPlayerOnMap, session, family, false);
            }
        }

        // Nosville
        if (session.CurrentMapInstance.MapId == 1)
        {
            foreach (IClientSession ses in _sessionManager.Sessions.Where(s => s.CurrentMapInstance.MapId.Equals(228)))
            {
                ses.SendTargetInPacket(session, _reputationConfiguration, _rankingManager.TopReputation, false, true);
                ses.SendPacket(session.GenerateClForcedPacket());

                session.SendTargetInPacket(ses, _reputationConfiguration, _rankingManager.TopReputation, false, true);
                session.SendPacket(ses.GenerateClForcedPacket());
            }
        }

        // Cylloan
        if (session.CurrentMapInstance.MapId == 228)
        {
            foreach (IClientSession ses in _sessionManager.Sessions.Where(s => s.CurrentMapInstance.MapId.Equals(1)))
            {
                ses.SendTargetInPacket(session, _reputationConfiguration, _rankingManager.TopReputation, false, true);
                ses.SendPacket(session.GenerateClForcedPacket());

                session.SendTargetInPacket(ses, _reputationConfiguration, _rankingManager.TopReputation, false, true);
                session.SendPacket(ses.GenerateClForcedPacket());
            }
        }
    }

    private async Task TargetedSendJoinInformation(IClientSession receiverSession, IClientSession targetSession, IFamily family, bool showInEffect)
    {
        bool isAnonymous = targetSession.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4)
            && receiverSession.PlayerEntity.Faction != targetSession.PlayerEntity.Faction && !receiverSession.IsGameMaster();

        receiverSession.SendTargetInPacket(targetSession, _reputationConfiguration, _rankingManager.TopReputation, isAnonymous, showInEffect);
        if (!isAnonymous)
        {
            receiverSession.SendTargetGidxPacket(targetSession, family, _languageService);
        }

        receiverSession.SendTargetTitInfoPacket(targetSession);
        receiverSession.SendTargetConstBuffEffects(targetSession.PlayerEntity);

        if (targetSession.PlayerEntity.HasShopOpened)
        {
            receiverSession.SendPlayerShopTitle(targetSession);
            receiverSession.SendPacket(targetSession.GeneratePlayerFlag((long)DialogVnums.SHOP_PLAYER));
        }

        if (targetSession.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        foreach (IMateEntity mate in targetSession.PlayerEntity.MateComponent.TeamMembers())
        {
            string inPacket = mate.GenerateIn(_languageService, receiverSession.UserLanguage, _spPartnerConfiguration, isAnonymous);
            receiverSession.SendPacket(inPacket);
            receiverSession.SendTargetConstBuffEffects(mate);
        }
    }
}