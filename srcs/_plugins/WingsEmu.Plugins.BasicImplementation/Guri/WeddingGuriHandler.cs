using System.Threading.Tasks;
using WingsAPI.Game.Extensions.RelationsExtensions;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Relations;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Relations;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class WeddingGuriHandler : IGuriHandler
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IInvitationManager _invitationManager;

    private readonly ISessionManager _sessionManager;

    public WeddingGuriHandler(ISessionManager sessionManager, IGameLanguageService gameLanguage, IInvitationManager invitationManager)
    {
        _sessionManager = sessionManager;
        _gameLanguage = gameLanguage;
        _invitationManager = invitationManager;
    }

    public long GuriEffectId => 603;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        if (e.User == null)
        {
            return;
        }

        long targetId = e.User.Value;

        if (!_invitationManager.ContainsPendingInvitation(targetId, session.PlayerEntity.Id, InvitationType.Spouse))
        {
            return;
        }

        _invitationManager.RemovePendingInvitation(targetId, session.PlayerEntity.Id, InvitationType.Spouse);

        IClientSession otherSession = _sessionManager.GetSessionByCharacterId(targetId);
        if (otherSession == null)
        {
            return;
        }

        switch (e.Data)
        {
            case 1:
                await session.RemoveRelationAsync(otherSession.PlayerEntity.Id, CharacterRelationType.Friend);
                await session.AddRelationAsync(otherSession.PlayerEntity.Id, CharacterRelationType.Spouse);
                _sessionManager.Broadcast(s =>
                {
                    string message = _gameLanguage.GetLanguageFormat(GameDialogKey.WEDDING_SHOUTMESSAGE_BROADCAST, s.UserLanguage, session.PlayerEntity.Name, otherSession.PlayerEntity.Name);
                    return s.GenerateMsgPacket(message, MsgMessageType.Middle);
                });
                await session.EmitEventAsync(new GroupWeedingEvent());

                break;

            case 0:
                _sessionManager.Broadcast(s =>
                {
                    string message = _gameLanguage.GetLanguageFormat(GameDialogKey.WEDDING_SHOUTMESSAGE_REFUSED_BROADCAST, s.UserLanguage, session.PlayerEntity.Name, otherSession.PlayerEntity.Name);
                    return s.GenerateMsgPacket(message, MsgMessageType.Middle);
                });
                break;
        }
    }
}