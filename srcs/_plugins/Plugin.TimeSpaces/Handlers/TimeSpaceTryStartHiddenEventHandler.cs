using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceTryStartHiddenEventHandler : IAsyncEventProcessor<TimeSpaceTryStartHiddenEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpaceTryStartHiddenEventHandler(IItemsManager itemsManager, IGameLanguageService gameLanguage, ITimeSpaceManager timeSpaceManager)
    {
        _itemsManager = itemsManager;
        _gameLanguage = gameLanguage;
        _timeSpaceManager = timeSpaceManager;
    }

    public async Task HandleAsync(TimeSpaceTryStartHiddenEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        INpcEntity timeSpacePortal = e.TimeSpacePortal;
        bool isChallengeMode = e.IsChallengeMode;
        bool isSoloTimeSpace = timeSpacePortal.TimeSpaceInfo.MinPlayers == 1 && timeSpacePortal.TimeSpaceInfo.MaxPlayers == 1;

        if (session.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            return;
        }

        if (session.IsMuted())
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.MUTE_SHOUTMESSAGE_YOU_ARE_MUTED), MsgMessageType.Middle);
            return;
        }

        if (!session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP), MsgMessageType.Middle);
            return;
        }

        TimeSpaceFileConfiguration timeSpace = timeSpacePortal.TimeSpaceInfo;

        if (timeSpace.SeedsOfPowerRequired != 0)
        {
            if (!session.PlayerEntity.HasItem((short)ItemVnums.SEED_OF_POWER, timeSpace.SeedsOfPowerRequired))
            {
                string itemName = _itemsManager.GetItem((short)ItemVnums.SEED_OF_POWER).GetItemName(_gameLanguage, session.UserLanguage);
                session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, timeSpace.SeedsOfPowerRequired, itemName), ChatMessageColorType.PlayerSay);
                return;
            }
        }

        if (isSoloTimeSpace)
        {
            if (session.PlayerEntity.Level < timeSpace.MinLevel)
            {
                session.SendChatMessage(session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_LOW_LEVEL), ChatMessageColorType.PlayerSay);
                return;
            }

            if (session.PlayerEntity.Level > timeSpace.MaxLevel)
            {
                session.SendChatMessage(session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_HIGH_LEVEL), ChatMessageColorType.PlayerSay);
                return;
            }

            if (e.IsChallengeMode)
            {
                if (!session.PlayerEntity.RemoveGold(timeSpace.MinLevel * 50))
                {
                    session.SendChatMessage(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD), ChatMessageColorType.PlayerSay);
                    return;
                }

                ;
            }

            if (timeSpace.SeedsOfPowerRequired != 0)
            {
                await session.RemoveItemFromInventory((short)ItemVnums.SEED_OF_POWER, timeSpace.SeedsOfPowerRequired);
            }

            var timeSpaceParty = new TimeSpaceParty(timeSpace, false, isChallengeMode);

            timeSpaceParty.AddMember(session);
            _timeSpaceManager.AddTimeSpace(timeSpaceParty);
            session.PlayerEntity.TimeSpaceComponent.SetTimeSpaceParty(timeSpaceParty);

            await session.EmitEventAsync(new TimeSpaceInstanceStartEvent());
            return;
        }

        if (!session.PlayerEntity.IsInGroup())
        {
            return;
        }

        PlayerGroup group = session.PlayerEntity.GetGroup();
        if (group.Members.Count < timeSpacePortal.TimeSpaceInfo.MinPlayers)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_NOT_ENOUGH_PLAYERS), MsgMessageType.Middle);
            return;
        }

        foreach (IPlayerEntity member in group.Members)
        {
            if (member.MapInstance.Id != session.CurrentMapInstance.Id)
            {
                session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_NOT_THE_SAME_MAP), MsgMessageType.Middle);
                return;
            }

            if (!member.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
            {
                session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_MEMBER_NOT_IN_CLASSIC_MAP), MsgMessageType.Middle);
                return;
            }
            INpcEntity timeSpaceNpc = member.MapInstance.GetPassiveNpcs().FirstOrDefault(x => x.TimeSpaceOwnerId.HasValue && member.Id == x.TimeSpaceOwnerId.Value);
            if (timeSpaceNpc == null || timeSpaceNpc.TimeSpaceOwnerId != member.Id)
                continue;
            if (member.Level < timeSpace.MinLevel)
            {
                member.Session.SendMsg(member.Session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_LOW_LEVEL), MsgMessageType.Middle);
                return;
            }

            if (member.Level > timeSpace.MaxLevel)
            {
                member.Session.SendMsg(member.Session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_HIGH_LEVEL), MsgMessageType.Middle);
                return;
            }
        }

        if (e.IsChallengeMode)
        {
            if (!session.PlayerEntity.RemoveGold(timeSpace.MinLevel * 50))
            {
                session.SendChatMessage(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD), ChatMessageColorType.PlayerSay);
                return;
            }

            ;
        }

        if (timeSpace.SeedsOfPowerRequired != 0)
        {
            await session.RemoveItemFromInventory((short)ItemVnums.SEED_OF_POWER, timeSpace.SeedsOfPowerRequired);
        }

        var timeSpacePartyGroup = new TimeSpaceParty(timeSpace, false, isChallengeMode);

        IEnumerable<IClientSession> members = group.Members.Select(x => x.Session).ToArray();
        foreach (IClientSession memberSession in members)
        {
            timeSpacePartyGroup.AddMember(memberSession);
        }

        _timeSpaceManager.AddTimeSpace(timeSpacePartyGroup);

        foreach (IPlayerEntity member in group.Members)
        {
            member.TimeSpaceComponent.SetTimeSpaceParty(timeSpacePartyGroup);
        }

        await session.EmitEventAsync(new TimeSpaceInstanceStartEvent());

        foreach (IPlayerEntity member in group.Members)
        {
            if (member.Id == session.PlayerEntity.Id)
            {
                continue;
            }

            IMapInstance mapStart = timeSpacePartyGroup.Instance.SpawnInstance.MapInstance;
            IClientSession memberSession = member.Session;

            memberSession.ChangeMap(mapStart, timeSpacePartyGroup.Instance.SpawnPoint.X, timeSpacePartyGroup.Instance.SpawnPoint.Y);
            memberSession.SendPacket(mapStart.GenerateRsfn(true, false));

            foreach (KeyValuePair<Guid, TimeSpaceSubInstance> mapInstance in timeSpacePartyGroup.Instance.TimeSpaceSubInstances)
            {
                memberSession.SendPacket(mapInstance.Value.MapInstance.GenerateRsfn(isVisit: false));
            }

            memberSession.SendRsfpPacket();
            mapStart.MapClear(true);
            mapStart.BroadcastTimeSpacePartnerInfo();
            memberSession.SendRsfmPacket(TimeSpaceAction.CAMERA_ADJUST);
            memberSession.SendMinfo();

            if (memberSession.PlayerEntity.Level > timeSpacePartyGroup.HigherLevel)
            {
                timeSpacePartyGroup.HigherLevel = memberSession.PlayerEntity.Level;
            }
        }
    }
}