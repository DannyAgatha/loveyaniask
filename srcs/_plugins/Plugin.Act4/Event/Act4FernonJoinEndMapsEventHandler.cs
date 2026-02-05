using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps.Event;

namespace Plugin.Act4.Event
{
    public class Act4FernonJoinEndMapsEventHandler : IAsyncEventProcessor<JoinMapEndEvent>
    {
        private readonly IAct4CaligorManager _manager;

        public Act4FernonJoinEndMapsEventHandler(IAct4CaligorManager manager)
        {
            _manager = manager;
        }

        public async Task HandleAsync(JoinMapEndEvent e, CancellationToken cancellation)
        {
            if (e.JoinedMapInstance.MapVnum != 250 || _manager.FernonMapsActive)
            {
                return;
            }

            // Tp back if Fernon its ended
            e.Sender.ChangeMap(153, 93, 93);
        }
    }
}