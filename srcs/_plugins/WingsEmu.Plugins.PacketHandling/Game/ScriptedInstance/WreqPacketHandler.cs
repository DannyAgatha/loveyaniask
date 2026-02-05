using System;
using System.Linq;
using System.Threading.Tasks;
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

public class WreqPacketHandler : GenericGamePacketHandlerBase<WreqPacket>
{
    private readonly ITimeSpaceService _timeSpaceService;

    public WreqPacketHandler(ITimeSpaceService timeSpaceService) => _timeSpaceService = timeSpaceService;

    protected override async Task HandlePacketAsync(IClientSession session, WreqPacket packet)
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

        INpcEntity npcTimeSpacePortal = session.CurrentMapInstance.GetPassiveNpcs().FirstOrDefault(s =>
            session.PlayerEntity.PositionY >= s.Position.Y - 1 &&
            session.PlayerEntity.PositionY <= s.Position.Y + 1 &&
            session.PlayerEntity.PositionX >= s.Position.X - 1 &&
            session.PlayerEntity.PositionX <= s.Position.X + 1 && s.TimeSpaceOwnerId.HasValue);

        if (npcTimeSpacePortal != null)
        {
            await HandleNpcTimeSpacePortal(session, npcTimeSpacePortal, packet);
            return;
        }

        ITimeSpacePortalEntity portal = session.CurrentMapInstance.TimeSpacePortals.FirstOrDefault(s =>
            session.PlayerEntity.PositionY >= s.Position.Y - 1 &&
            session.PlayerEntity.PositionY <= s.Position.Y + 1 &&
            session.PlayerEntity.PositionX >= s.Position.X - 1 &&
            session.PlayerEntity.PositionX <= s.Position.X + 1);

        if (portal == null)
        {
            return;
        }

        if (session.PlayerEntity.LastAttack.AddSeconds(5) > DateTime.UtcNow)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_ATTACK_COOLDOWN), ChatMessageColorType.Yellow);
            return;
        }

        switch (packet.Value)
        {
            case 0:
                IPlayerEntity findMemberInTimeSpace = session.PlayerEntity.GetGroup()?.Members
                    .FirstOrDefault(x => x.TimeSpaceComponent.TimeSpace?.Instance != null && x.TimeSpaceComponent.TimeSpace.TimeSpaceId == portal.TimeSpaceId);

                if (findMemberInTimeSpace != null && !packet.Param.HasValue)
                {
                    session.SendDialog($"wreq 3 {findMemberInTimeSpace.Id}", "wreq 0 1", session.GetLanguage(GameDialogKey.TIMESPACE_DIALOG_ASK_JOIN_TO_PLAYER));
                    return;
                }

                TimeSpaceRecordResponse record = await _timeSpaceService.GetTimeSpaceRecord(new TimeSpaceRecordRequest
                {
                    TimeSpaceId = portal.TimeSpaceId
                });

                session.SendTimeSpaceInfo(portal, record?.TimeSpaceRecordDto);
                break;
            case 1:
                if (packet.Param is 1)
                {
                    session.SendQnaPacket("wreq 1 0", session.GetLanguageFormat(GameDialogKey.TIMESPACE_DIALOG_ASK_START_RECORD, portal.MinLevel * 50));
                    return;
                }

                await session.EmitEventAsync(new TimeSpacePartyCreateEvent(portal.TimeSpaceId, null, false, packet.Param is 0));
                if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
                {
                    return;
                }

                await session.EmitEventAsync(new TimeSpaceInstanceStartEvent());
                break;
            case 3:
                if (!packet.Param.HasValue)
                {
                    return;
                }

                await session.EmitEventAsync(new TimeSpaceGroupTryJoinEvent
                {
                    CharacterId = packet.Param.Value,
                    PortalEntity = portal
                });
                break;
            case 5:
                if (portal.IsHidden || portal.IsSpecial)
                {
                    return;
                }

                await session.EmitEventAsync(new TimeSpacePartyCreateEvent(portal.TimeSpaceId, null, true));
                if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
                {
                    return;
                }

                await session.EmitEventAsync(new TimeSpaceInstanceStartEvent());
                break;
        }
    }

    private async Task HandleNpcTimeSpacePortal(IClientSession session, INpcEntity npcTimeSpacePortal, WreqPacket clientPacket)
    {
        if (npcTimeSpacePortal.Position.GetDistance(session.PlayerEntity.Position) > 5)
        {
            return;
        }

        if (npcTimeSpacePortal.TimeSpaceInfo == null || npcTimeSpacePortal.TimeSpaceOwnerId == null)
        {
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

        switch (clientPacket.Value)
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

                if (clientPacket.Param is 1)
                {
                    session.SendQnaPacket("wreq 1 0",
                        session.GetLanguageFormat(GameDialogKey.TIMESPACE_DIALOG_ASK_START_RECORD, npcTimeSpacePortal.TimeSpaceInfo.MinLevel * 50));
                    return;
                }

                await session.EmitEventAsync(new TimeSpaceTryStartHiddenEvent
                {
                    TimeSpacePortal = npcTimeSpacePortal,
                    IsChallengeMode = clientPacket.Param is 0
                });
                break;
        }
    }
}