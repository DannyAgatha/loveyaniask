using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Packets.Enums;

namespace Plugin.Act4.Event
{
    public class Act4CaligorAddFactionPointEventHandler : IAsyncEventProcessor<Act4CaligorAddFactionPointEvent>
    {
        private readonly IAct4CaligorManager _manager;
        public Act4CaligorAddFactionPointEventHandler(IAct4CaligorManager manager)
        {
            _manager = manager;
        }

        public async Task HandleAsync(Act4CaligorAddFactionPointEvent e, CancellationToken cancellation)
        {
            if (!_manager.CaligorActive)
            {
                return;
            }
            if (e.Faction == FactionType.Angel)
            {
                _manager.AngelDamage += e.Damage;
                return;
            }
            _manager.DemonDamage += e.Damage;
        }
    }
}