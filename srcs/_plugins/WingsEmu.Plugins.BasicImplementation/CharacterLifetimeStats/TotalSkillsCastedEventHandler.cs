using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;

namespace NosEmu.Plugins.BasicImplementations.CharacterLifetimeStats;

public class TotalSkillsCastedEventHandler : IAsyncEventProcessor<BattleExecuteSkillEvent>
{
    public async Task HandleAsync(BattleExecuteSkillEvent e, CancellationToken cancellation)
    {
        if (e.Entity is not IPlayerEntity playerEntity)
        {
            return;
        }

        playerEntity.LifetimeStats.TotalSkillsCasted++;
    }
}