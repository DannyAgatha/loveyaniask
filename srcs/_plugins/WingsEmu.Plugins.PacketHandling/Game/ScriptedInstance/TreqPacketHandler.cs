using System;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Communication.DbServer.TimeSpaceService;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Portals;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.ScriptedInstance;

public class TReqPacketHandler : GenericGamePacketHandlerBase<TreqClientPacket>
{
    private readonly ITimeSpaceService _timeSpaceService;

    public TReqPacketHandler(ITimeSpaceService timeSpaceService) => _timeSpaceService = timeSpaceService;

    protected override async Task HandlePacketAsync(IClientSession session, TreqClientPacket clientPacket)
    {
        if (!session.HasCurrentMapInstance)
        {
            return;
        }

        if (session.PlayerEntity.IsSeal)
        {
            return;
        }

        if (!session.PlayerEntity.IsAlive())
        {
            return;
        }

        if (session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        INpcEntity npcTimeSpacePortal = session.CurrentMapInstance.GetPassiveNpcs().FirstOrDefault(x => x.TimeSpaceInfo != null &&
            clientPacket.X == x.PositionX && clientPacket.Y == x.PositionY);
        if (npcTimeSpacePortal != null)
        {
            await HandleNpcTimeSpacePortal(session, npcTimeSpacePortal, clientPacket);
            return;
        }

        ITimeSpacePortalEntity portal = session.CurrentMapInstance.TimeSpacePortals.FirstOrDefault(s =>
            clientPacket.X == s.Position.X && clientPacket.Y == s.Position.Y);

        if (portal == null)
        {
            return;
        }

        if (portal.Position.GetDistance(session.PlayerEntity.Position) > 5)
        {
            return;
        }

        switch (clientPacket.StartPress)
        {
            case 0:
                IPlayerEntity findMemberInTimeSpace = session.PlayerEntity.GetGroup()?.Members
                    .FirstOrDefault(x => x.TimeSpaceComponent.TimeSpace?.Instance != null && x.TimeSpaceComponent.TimeSpace.TimeSpaceId == portal.TimeSpaceId);

                if (findMemberInTimeSpace != null && clientPacket.RecordPressAndCharacterId == 0)
                {
                    session.SendDialog($"treq {portal.Position.X} {portal.Position.Y} 3 {findMemberInTimeSpace.Id}", $"treq {portal.Position.X} {portal.Position.Y} 0 1",
                        session.GetLanguage(GameDialogKey.TIMESPACE_DIALOG_ASK_JOIN_TO_PLAYER));
                    return;
                }

                TimeSpaceRecordResponse record = await _timeSpaceService.GetTimeSpaceRecord(new TimeSpaceRecordRequest
                {
                    TimeSpaceId = portal.TimeSpaceId
                });

                session.SendTimeSpaceInfo(portal, record?.TimeSpaceRecordDto);
                break;
            case 1:
                if (clientPacket.RecordPressAndCharacterId == 1 && !clientPacket.RecordPressConfirm)
                {
                    session.SendQnaPacket($"treq {portal.Position.X} {portal.Position.Y} 1 0 1", session.GetLanguageFormat(GameDialogKey.TIMESPACE_DIALOG_ASK_START_RECORD, portal.MinLevel * 50));
                    return;
                }

                await session.EmitEventAsync(new TimeSpacePartyCreateEvent(portal.TimeSpaceId, null, false, clientPacket.RecordPressConfirm));
                if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
                {
                    return;
                }

                await session.EmitEventAsync(new TimeSpaceInstanceStartEvent());
                break;
            case 3:
                if (clientPacket.RecordPressAndCharacterId == 0)
                {
                    return;
                }

                await session.EmitEventAsync(new TimeSpaceGroupTryJoinEvent
                {
                    CharacterId = clientPacket.RecordPressAndCharacterId,
                    PortalEntity = portal
                });
                break;
            case 5:
                await session.EmitEventAsync(new TimeSpacePartyCreateEvent(portal.TimeSpaceId, null, true));
                if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
                {
                    return;
                }

                await session.EmitEventAsync(new TimeSpaceInstanceStartEvent());
                break;
        }
    }

    private async Task HandleNpcTimeSpacePortal(IClientSession session, INpcEntity npcTimeSpacePortal, TreqClientPacket clientPacket)
    {
        if (npcTimeSpacePortal.Position.GetDistance(session.PlayerEntity.Position) > 5)
        {
            return;
        }

        if (npcTimeSpacePortal.TimeSpaceInfo == null)
        {
            Log.Error("TimeSpaceInfo couldn't be find in this TimeSpacePortal", new Exception());
            return;
        }

        if (npcTimeSpacePortal.TimeSpaceOwnerId == null)
        {
            Log.Error("TimeSpacePortal doesn't have any owner", new Exception());
            return;
        }

        bool isSoloTimeSpace = npcTimeSpacePortal.TimeSpaceInfo.MinPlayers == 1 && npcTimeSpacePortal.TimeSpaceInfo.MaxPlayers == 1;
        switch (isSoloTimeSpace)
        {
            // Solo Time-Space and player is in group
            case true when session.PlayerEntity.IsInGroup():
                session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_CANT_BE_IN_GROUP), MsgMessageType.Middle);
                return;
            // Group Time-Space and player is not in group
            case false when !session.PlayerEntity.IsInGroup():
                session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_MUST_BE_IN_GROUP), MsgMessageType.Middle);
                return;
        }

        if (session.PlayerEntity.Id != npcTimeSpacePortal.TimeSpaceOwnerId.Value)
        {
            if (!session.PlayerEntity.IsInGroup())
            {
                return;
            }

            bool timeSpaceOwnerInGroup = session.PlayerEntity.GetGroup().Members.Any(member => member.Id == npcTimeSpacePortal.TimeSpaceOwnerId.Value);
            if (!timeSpaceOwnerInGroup)
            {
                return;
            }
        }

        switch (clientPacket.StartPress)
        {
            case 0:
                TimeSpaceRecordResponse record = await _timeSpaceService.GetTimeSpaceRecord(new TimeSpaceRecordRequest
                {
                    TimeSpaceId = npcTimeSpacePortal.TimeSpaceInfo.TsId
                });

                session.SendTimeSpaceInfo(npcTimeSpacePortal, record?.TimeSpaceRecordDto);
                break;
            case 1:
                if (session.PlayerEntity.Id != npcTimeSpacePortal.TimeSpaceOwnerId.Value)
                {
                    session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_NOT_OWNER), MsgMessageType.Middle);
                    return;
                }

                if (clientPacket.RecordPressAndCharacterId == 1 && !clientPacket.RecordPressConfirm)
                {
                    session.SendQnaPacket($"treq {npcTimeSpacePortal.Position.X} {npcTimeSpacePortal.Position.Y} 1 0 1",
                        session.GetLanguageFormat(GameDialogKey.TIMESPACE_DIALOG_ASK_START_RECORD, npcTimeSpacePortal.TimeSpaceInfo.MinLevel * 50));
                    return;
                }

                await session.EmitEventAsync(new TimeSpaceTryStartHiddenEvent
                {
                    TimeSpacePortal = npcTimeSpacePortal,
                    IsChallengeMode = clientPacket.RecordPressConfirm
                });
                break;
        }
    }
}