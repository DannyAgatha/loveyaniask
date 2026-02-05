using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.Relations;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids;

public class RaidPartyInvitePlayerEventHandler : IAsyncEventProcessor<RaidPartyInvitePlayerEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IInvitationManager _invitationManager;
    private readonly ISessionManager _sessionManager;

    public RaidPartyInvitePlayerEventHandler(ISessionManager sessionManager, IGameLanguageService gameLanguage, IInvitationManager invitationManager)
    {
        _sessionManager = sessionManager;
        _gameLanguage = gameLanguage;
        _invitationManager = invitationManager;
    }

    public async Task HandleAsync(RaidPartyInvitePlayerEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        long targetId = e.TargetId;

        if (session.PlayerEntity.Id == targetId)
        {
            return;
        }

        if (!session.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        if (session.PlayerEntity.HasRaidStarted)
        {
            return;
        }

        if (!session.PlayerEntity.IsRaidLeader(session.PlayerEntity.Id))
        {
            return;
        }

        if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.ArenaInstance)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.RAID_SHOUTMESSAGE_CANT_JOIN_FROM_ARENA, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        IClientSession target = _sessionManager.GetSessionByCharacterId(targetId);
        if (target == null)
        {
            return;
        }

        if (session.PlayerEntity.IsBlocking(targetId))
        {
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.BLACKLIST_INFO_BLOCKING, session.UserLanguage));
            return;
        }

        if (target.PlayerEntity.IsBlocking(session.PlayerEntity.Id))
        {
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.BLACKLIST_INFO_BLOCKED, session.UserLanguage));
            return;
        }

        if (target.PlayerEntity.IsInGroup())
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.RAID_SHOUTMESSAGE_CANT_INVITE_GROUP, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (target.IsMuted())
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.MUTE_SHOUTMESSAGE_PLAYER_IS_MUTED), MsgMessageType.Middle);
            return;
        }

        if (target.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        if (target.PlayerEntity.HasRaidStarted)
        {
            return;
        }

        if (target.PlayerEntity.HasShopOpened)
        {
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_USER_NOT_BASEMAP, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.RAID_CHATMESSAGE_GROUP_REQUEST, session.UserLanguage, target.PlayerEntity.Name), ChatMessageColorType.Yellow);
        await session.EmitEventAsync(new InvitationEvent(targetId, InvitationType.Raid));
    }
}