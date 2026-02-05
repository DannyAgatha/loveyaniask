using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Services.Messages;

namespace FamilyServer.Consumers
{
    public class ServiceFlushAllMessageConsumer : IMessageConsumer<ServiceFlushAllMessage>
    {
        private readonly FamilySystem _familySystem;

        public ServiceFlushAllMessageConsumer(FamilySystem familySystem) => _familySystem = familySystem;

        public async Task HandleAsync(ServiceFlushAllMessage notification, CancellationToken token)
        {
            await _familySystem.ProcessMain();
        }
    }
}