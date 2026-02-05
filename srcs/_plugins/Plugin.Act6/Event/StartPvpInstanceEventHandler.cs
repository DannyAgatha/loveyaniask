using PhoenixLib.Events;
using WingsEmu.Game.Act6;
using WingsEmu.Game.Act6.Event;
using WingsEmu.Packets.Enums;

namespace Plugin.Act6.Event
{
    public class StartPvpInstanceEventHandler : IAsyncEventProcessor<StartPvpInstanceEvent>
    {
        private readonly IAct6InstanceManager _act6Manager;

        public StartPvpInstanceEventHandler(IAct6InstanceManager act6Manager) => _act6Manager = act6Manager;

        public async Task HandleAsync(StartPvpInstanceEvent e, CancellationToken cancellation)
        {
            if (_act6Manager.PvpInstance.InstanceActive)
            {
                return;
            }

            _act6Manager.EnablePvpInstance();
        }
    }
}