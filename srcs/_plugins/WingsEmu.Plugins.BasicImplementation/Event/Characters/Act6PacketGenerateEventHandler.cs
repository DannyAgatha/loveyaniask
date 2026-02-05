using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Act6.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters
{
    public class Act6PacketGenerateEventHandler : IAsyncEventProcessor<Act6PacketGenerateEvent>
    {
        private readonly IAct6Manager _act6Manager;

        public Act6PacketGenerateEventHandler(IAct6Manager act6Manager)
        {
            _act6Manager = act6Manager;
        }

        public async Task HandleAsync(Act6PacketGenerateEvent e, CancellationToken cancellation)
        {
            Act6Status status = _act6Manager.GetStatus();
            e.Sender.SendPacket(UiPacketExtension.GenerateAct6PacketUi(status));
        }
    }
}