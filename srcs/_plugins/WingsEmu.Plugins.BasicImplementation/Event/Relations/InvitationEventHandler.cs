using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Exchange.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.Relations;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Relations;

public class InvitationEventHandler : IAsyncEventProcessor<InvitationEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IInvitationManager _invitationManager;
    private readonly ISessionManager _sessionManager;

    public InvitationEventHandler(IInvitationManager invitationManager, ISessionManager sessionManager, IGameLanguageService gameLanguage)
    {
        _invitationManager = invitationManager;
        _sessionManager = sessionManager;
        _gameLanguage = gameLanguage;
    }

    public async Task HandleAsync(InvitationEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IClientSession otherSession = _sessionManager.GetSessionByCharacterId(e.TargetCharacterId);
        if (otherSession == null)
        {
            return;
        }

        bool invitationExist = _invitationManager.ContainsPendingInvitation(session.PlayerEntity.Id, e.TargetCharacterId, e.Type);
        if (invitationExist)
        {
            return;
        }

        switch (e.Type)
        {
            case InvitationType.Friend:
                otherSession.SendDialog($"fins 2 {session.PlayerEntity.Id}", $"fins 0 {session.PlayerEntity.Id}",
                    _gameLanguage.GetLanguageFormat(GameDialogKey.FRIEND_DIALOG_DO_YOU_WANT_TO_ADD, otherSession.UserLanguage, session.PlayerEntity.Name));
                break;
            case InvitationType.HiddenSpouse:
                break;
            case InvitationType.Spouse:
                otherSession.SendDialog($"guri 603 1 {session.PlayerEntity.Id}", $"guri 603 0 {session.PlayerEntity.Id}",
                    _gameLanguage.GetLanguageFormat(GameDialogKey.WEDDING_DIALOG_REQUEST_RECEIVED, otherSession.UserLanguage, session.PlayerEntity.Name));
                break;
            case InvitationType.FamilySummoningHorn:
                otherSession.SendDialog($"guri 10000 1 {session.PlayerEntity.Id}", $"guri 10000 0 {session.PlayerEntity.Id}",
                    _gameLanguage.GetLanguageFormat(GameDialogKey.FAMILY_INFO_HORN_SUMMON_ASK, otherSession.UserLanguage, session.PlayerEntity.Name));
                break;
            case InvitationType.Group:
                otherSession.SendDialog($"pjoin 3 {session.PlayerEntity.Id}", $"pjoin 4 {session.PlayerEntity.Id}",
                    $"{_gameLanguage.GetLanguageFormat(GameDialogKey.GROUP_DIALOG_INVITED_YOU, otherSession.UserLanguage, session.PlayerEntity.Name, session.PlayerEntity.Level, session.PlayerEntity.HeroLevel)}");
                await session.EmitEventAsync(new GroupInvitedEvent { TargetId = e.TargetCharacterId });
                break;
            case InvitationType.Exchange:
                otherSession.SendDialog(
                    $"req_exc 2 {session.PlayerEntity.Id}",
                    $"req_exc 5 {session.PlayerEntity.Id}",
                    _gameLanguage.GetLanguageFormat(GameDialogKey.TRADE_DIALOG_INCOMING_EXCHANGE, otherSession.UserLanguage, session.PlayerEntity.Name, session.PlayerEntity.Level,
                        session.PlayerEntity.HeroLevel)
                );
                await session.EmitEventAsync(new TradeRequestedEvent { TargetId = e.TargetCharacterId });
                break;
            case InvitationType.Raid:
                otherSession.SendDialog(
                    $"rd 1 {session.PlayerEntity.Id} 1", $"rd 1 {session.PlayerEntity.Id} 2",
                    _gameLanguage.GetLanguageFormat(GameDialogKey.RAID_DIALOG_INVITED_YOU, otherSession.UserLanguage, session.PlayerEntity.Name)
                );
                await session.EmitEventAsync(new RaidInvitedEvent { TargetId = e.TargetCharacterId });
                break;
            case InvitationType.GroupPointShare:
                otherSession.SendDialog(
                    $"pjoin 6 {session.PlayerEntity.Id}", $"pjoin 7 {session.PlayerEntity.Id}",
                    _gameLanguage.GetLanguageFormat(GameDialogKey.GROUP_DIALOG_ASK_SHARE_POINT, otherSession.UserLanguage, session.PlayerEntity.Name)
                );
                break;
        }

        _invitationManager.AddPendingInvitation(session.PlayerEntity.Id, e.TargetCharacterId, e.Type);
    }
}