using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Act6;
using WingsEmu.Game.Act6.Event;
using WingsEmu.Game.Groups.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters
{
    internal class Act6CheckGroupEventHandler : IAsyncEventProcessor<Act6CheckGroupEvent>
    {
        private readonly IAct6InstanceManager _act6InstanceManager;

        public Act6CheckGroupEventHandler(IAct6InstanceManager act6InstanceManager)
        {
            _act6InstanceManager = act6InstanceManager;
        }

        public async Task HandleAsync(Act6CheckGroupEvent e, CancellationToken cancellation)
        {
            if (_act6InstanceManager.PvpMap == null || e.Sender.CurrentMapInstance.Id != _act6InstanceManager.PvpMap.Id || !e.Sender.PlayerEntity.IsInGroup())
            {
                return;
            }
            e.Sender.EmitEvent(new LeaveGroupEvent());
        }
    }
}