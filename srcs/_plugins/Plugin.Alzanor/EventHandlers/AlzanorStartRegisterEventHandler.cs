using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums;

namespace Plugin.Alzanor.EventHandlers;

public class AlzanorStartRegisterEventHandler : IAsyncEventProcessor<AlzanorStartRegisterEvent>
{
    private readonly IAlzanorManager _alzanorManager;
    private readonly ISessionManager _sessionManager;

    public AlzanorStartRegisterEventHandler(IAlzanorManager alzanorManager, ISessionManager sessionManager)
    {
        _alzanorManager = alzanorManager;
        _sessionManager = sessionManager;
    }

    public async Task HandleAsync(AlzanorStartRegisterEvent e, CancellationToken cancellation)
    {
        if (_alzanorManager.IsRegistrationActive)
        {
            return;
        }

        _alzanorManager.AlzanorProcessTime = null;
        _alzanorManager.EnableAlzanorRegistration();

        _sessionManager.Broadcast(x => x.GenerateEventAsk(QnamlType.MeteorInvRaid, "guri 509",
                x.GetLanguageFormat(GameDialogKey.GAMEEVENT_DIALOG_ASK_PARTICIPATE, x.GetLanguage(GameDialogKey.ALZANOR_EVENT_NAME))),
            new InBaseMapBroadcast(), new NotMutedBroadcast());
    }
}