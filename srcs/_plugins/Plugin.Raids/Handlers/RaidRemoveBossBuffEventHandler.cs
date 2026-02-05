using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids;

public class RaidRemoveBossBuffEventHandler : IAsyncEventProcessor<RaidRemoveBossBuffEvent>
{
    public async Task HandleAsync(RaidRemoveBossBuffEvent e, CancellationToken cancellation)
    {
        IMonsterEntity raidBoss = e.MapInstance.GetAliveMonsters().FirstOrDefault(x => x.IsBoss);
        if (raidBoss == null || !raidBoss.BuffComponent.HasBuff(e.BuffId)) return;
        await raidBoss.RemoveBuffAsync(e.BuffId);
    }
}