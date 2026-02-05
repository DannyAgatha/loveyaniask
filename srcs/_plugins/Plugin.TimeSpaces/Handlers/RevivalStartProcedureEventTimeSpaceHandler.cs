using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;

namespace Plugin.TimeSpaces.Handlers;

public class RevivalStartProcedureEventTimeSpaceHandler : IAsyncEventProcessor<RevivalStartProcedureEvent>
{
    public async Task HandleAsync(RevivalStartProcedureEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        if (session.PlayerEntity.IsAlive())
        {
            return;
        }

        DateTime currentTime = DateTime.UtcNow;
        e.Sender.PlayerEntity.UpdateRevival(currentTime + TimeSpan.FromSeconds(10), RevivalType.DontPayRevival, ForcedType.Reconnect);
        e.Sender.PlayerEntity.UpdateAskRevival(currentTime + TimeSpan.FromSeconds(2), AskRevivalType.TimeSpaceRevival);
    }
}