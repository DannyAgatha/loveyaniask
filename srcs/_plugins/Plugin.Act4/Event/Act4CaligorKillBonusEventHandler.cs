using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Maps;

namespace Plugin.Act4.Event
{
    public class Act4CaligorKillBonusEventHandler : IAsyncEventProcessor<KillBonusEvent>
    {
        private readonly IAct4CaligorManager _act4CaligorManager;
        public Act4CaligorKillBonusEventHandler(IAct4CaligorManager act4CaligorManager)
        {
            _act4CaligorManager = act4CaligorManager;
        }

        public async Task HandleAsync(KillBonusEvent e, CancellationToken cancellation)
        {
            IMonsterEntity monsterEntityToAttack = e.MonsterEntity;

            if (monsterEntityToAttack == null || monsterEntityToAttack.IsStillAlive)
            {
                return;
            }

            if (e.Sender.CurrentMapInstance.MapInstanceType != MapInstanceType.Caligor || monsterEntityToAttack.MonsterVNum != 2305)
            {
                return;
            }

            _act4CaligorManager.EndCaligorInstance(true);
        }
    }
}