using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers;

namespace Plugin.Act4.Event;

public sealed class Act4KillBonusEventHandler : IAsyncEventProcessor<KillBonusEvent>
{
    private readonly Act4Configuration _act4Configuration;
    private readonly IAct4Manager _act4Manager;

    public Act4KillBonusEventHandler(IAct4Manager act4Manager, Act4Configuration act4Configuration)
    {
        _act4Manager = act4Manager;
        _act4Configuration = act4Configuration;
    }

    public async Task HandleAsync(KillBonusEvent e, CancellationToken cancellation)
    {
        IMonsterEntity monsterEntityToAttack = e.MonsterEntity;

        if (monsterEntityToAttack == null)
        {
            return;
        }

        if (monsterEntityToAttack.IsStillAlive)
        {
            return;
        }

        if (!monsterEntityToAttack.MapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            return;
        }

        if (monsterEntityToAttack.MapInstance.HasMapFlag(MapFlags.HAS_PVE_REPUTATION_ENABLED))
        {
            await e.Sender.EmitEventAsync(new GenerateReputationEvent
            {
                Amount = monsterEntityToAttack.Level / 2,
                SendMessage = true
            });
        }

        if (_act4Manager.FactionPointsLocked || !_act4Configuration.PveFactionPoints)
        {
            return;
        }

        await e.Sender.EmitEventAsync(new Act4FactionPointsIncreaseEvent(_act4Configuration.FactionPointsPerPveKill));
    }
}