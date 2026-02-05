using PhoenixLib.Events;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Act6.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking.Broadcasting;

namespace Plugin.Act6.Event
{
    internal class Act6SystemBroadcastEventHandler : IAsyncEventProcessor<Act6SystemBroadcastEvent>
    {
        private readonly IAct6Manager _act6Manager;
        private readonly ISessionManager _sessionManager;

        public Act6SystemBroadcastEventHandler(IAct6Manager act6Manager, ISessionManager sessionManager)
        {
            _act6Manager = act6Manager;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(Act6SystemBroadcastEvent e, CancellationToken cancellation)
        {
            if (_act6Manager.AngelFaction.CurrentTime <= 0 && _act6Manager.AngelFaction.Mode != 0)
            {
                _act6Manager.AngelFaction.Mode = 0;
                _act6Manager.AngelFaction.TotalTime = 0;
            }
            else if (_act6Manager.DemonFaction.CurrentTime <= 0 && _act6Manager.DemonFaction.Mode != 0)
            {
                _act6Manager.DemonFaction.Mode = 0;
                _act6Manager.DemonFaction.TotalTime = 0;
            }
            
            Act6Status status = _act6Manager.GetStatus();
            string packet = UiPacketExtension.GenerateAct6PacketUi(status);
            _sessionManager.Broadcast(packet, new InMapFlagBrodcast(MapFlags.ACT_6_1));
        }
    }
}