using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Mates.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.Mates;

public class MateBackToMinilandEventHandler : IAsyncEventProcessor<MateBackToMinilandEvent>
{
    private readonly MateReviveEventHandler _mateReviveEventHandler;

    public MateBackToMinilandEventHandler(MateReviveEventHandler mateReviveEventHandler) => _mateReviveEventHandler = mateReviveEventHandler;

    public async Task HandleAsync(MateBackToMinilandEvent e, CancellationToken cancellation)
    {
        if (!e.MateEntity.IsTeamMember || e.Sender.PlayerEntity?.MapInstance == null)
        {
            return;
        }

        if (!_mateReviveEventHandler.BasicUnregisteringForMates(e.MateEntity, true, e.ExpectedGuid))
        {
            return;
        }

        await e.Sender.EmitEventAsync(new MateLeaveTeamEvent
        {
            MateEntity = e.MateEntity
        });
        e.MateEntity.Hp = 1;
        e.MateEntity.Mp = 1;
    }
}