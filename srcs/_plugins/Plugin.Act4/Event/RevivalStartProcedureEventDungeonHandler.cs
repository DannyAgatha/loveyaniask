using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Revival;

namespace Plugin.Act4.Event;

public class RevivalStartProcedureEventDungeonHandler : IAsyncEventProcessor<RevivalStartProcedureEvent>
{
    private readonly Act4DungeonsConfiguration _act4DungeonsConfiguration;
    private readonly PlayerRevivalConfiguration _revivalConfiguration;

    public RevivalStartProcedureEventDungeonHandler(Act4DungeonsConfiguration act4DungeonsConfiguration, GameRevivalConfiguration gameRevivalConfiguration)
    {
        _act4DungeonsConfiguration = act4DungeonsConfiguration;
        _revivalConfiguration = gameRevivalConfiguration.PlayerRevivalConfiguration;
    }

    public async Task HandleAsync(RevivalStartProcedureEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.IsAlive() || e.Sender.CurrentMapInstance.MapInstanceType != MapInstanceType.Act4Dungeon)
        {
            return;
        }

        DateTime currentTime = DateTime.UtcNow;
        e.Sender.PlayerEntity.UpdateRevival(currentTime + _act4DungeonsConfiguration.DungeonDeathRevivalDelay, RevivalType.DontPayRevival, ForcedType.Reconnect);
        e.Sender.PlayerEntity.UpdateAskRevival(currentTime + _revivalConfiguration.RevivalDialogDelay, AskRevivalType.DungeonRevival);
    }
}