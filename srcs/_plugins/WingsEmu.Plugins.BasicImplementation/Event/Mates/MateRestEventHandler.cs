using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.Mates;

public class MateRestEventHandler : IAsyncEventProcessor<MateRestEvent>
{
    public async Task HandleAsync(MateRestEvent e, CancellationToken cancellation)
    {
        IMateEntity mateEntity = e.MateEntity;

        if (mateEntity == null)
        {
            return;
        }

        if (mateEntity.LastSkillUse.AddSeconds(4) > DateTime.UtcNow && !e.Force)
        {
            return;
        }

        if (mateEntity.LastDefence.AddSeconds(4) > DateTime.UtcNow && !e.Force)
        {
            return;
        }

        if (!e.Sender.HasCurrentMapInstance)
        {
            return;
        }

        mateEntity.IsSitting = e.Rest;
        e.Sender.Broadcast(mateEntity.GenerateRest());
    }
}