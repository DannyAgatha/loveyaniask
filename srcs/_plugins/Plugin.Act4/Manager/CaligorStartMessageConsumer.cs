using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.Caligor;
using WingsEmu.Game.Act4;

namespace Plugin.Act4.Manager
{
    internal class CaligorStartMessageConsumer : IMessageConsumer<CaligorStartMessage>
    {
        private readonly IAct4CaligorManager _manager;
        
        public CaligorStartMessageConsumer(IAct4CaligorManager manager)
        {
            _manager = manager;
        }

        public async Task HandleAsync(CaligorStartMessage notification, CancellationToken token)
        {
            if (_manager.CaligorActive)
            {
                return;
            }
            _manager.InitializeAndStartCaligorInstance();
        }
    }
}