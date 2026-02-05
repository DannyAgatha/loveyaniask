using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Relations;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class FamilyHornGuriHandler : IGuriHandler
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IInvitationManager _invitationManager;
    private readonly ISessionManager _sessionManager;

    public FamilyHornGuriHandler(IGameLanguageService gameLanguage, IInvitationManager invitationManager, ISessionManager sessionManager)
    {
        _gameLanguage = gameLanguage;
        _invitationManager = invitationManager;
        _sessionManager = sessionManager;
    }

    public long GuriEffectId => 10000;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        if (e.User == null)
        {
            return;
        }

        long targetId = e.User.Value;

        if (!_invitationManager.ContainsPendingInvitation(targetId, session.PlayerEntity.Id, InvitationType.FamilySummoningHorn))
        {
            return;
        }

        _invitationManager.RemovePendingInvitation(targetId, session.PlayerEntity.Id, InvitationType.FamilySummoningHorn);

        IClientSession otherSession = _sessionManager.GetSessionByCharacterId(targetId);
        if (otherSession == null)
        {
            return;
        }

        if (e.Data == 1)
        {
            session.ChangeMap(otherSession.PlayerEntity.MapInstance, otherSession.PlayerEntity.PositionX, otherSession.PlayerEntity.PositionY);
        }
    }
}