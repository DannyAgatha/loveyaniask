using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Revival;

namespace NosEmu.Plugins.BasicImplementations.Revival;

public class RevivalStartProcedureEventUnderwaterShowdownHandler : IAsyncEventProcessor<RevivalStartProcedureEvent>
{
    public async Task HandleAsync(RevivalStartProcedureEvent e, CancellationToken cancellation)
    {
        if (e.Sender.CurrentMapInstance.MapInstanceType is not MapInstanceType.UnderWaterShowdown)
        {
            return;
        }
        
        e.Sender.PlayerEntity.UpdateRevival(DateTime.UtcNow + TimeSpan.FromSeconds(3), RevivalType.TryPayRevival, ForcedType.Forced);
    }
}