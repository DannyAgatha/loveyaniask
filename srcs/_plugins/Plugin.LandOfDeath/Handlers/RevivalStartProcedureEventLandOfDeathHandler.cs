using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Revival;

namespace Plugin.LandOfDeath.Handlers;

public class RevivalStartProcedureEventLandOfDeathHandler : IAsyncEventProcessor<RevivalStartProcedureEvent>
{
    private readonly GameRevivalConfiguration _revivalConfiguration;

    public RevivalStartProcedureEventLandOfDeathHandler(GameRevivalConfiguration revivalConfiguration)
    {
        _revivalConfiguration = revivalConfiguration;
    }

    public async Task HandleAsync(RevivalStartProcedureEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.IsAlive() || e.Sender.CurrentMapInstance.MapInstanceType != MapInstanceType.LandOfDeath)
        {
            return;
        }
        
        DateTime actualTime = DateTime.UtcNow;

        if (e.Sender.PlayerEntity.IsOnVehicle)
        {
            await e.Sender.EmitEventAsync(new RemoveVehicleEvent());
        }
        
        await e.Sender.PlayerEntity.RemoveBuffsOnDeathAsync();
        e.Sender.RefreshStat();

        e.Sender.PlayerEntity.UpdateAskRevival(actualTime + _revivalConfiguration.PlayerRevivalConfiguration.RevivalDialogDelay, AskRevivalType.LandOfDeathRevival);
        e.Sender.PlayerEntity.UpdateRevival(actualTime + _revivalConfiguration.PlayerRevivalConfiguration.ForcedRevivalDelay, RevivalType.TryPayRevival, ForcedType.Forced);
    }
}