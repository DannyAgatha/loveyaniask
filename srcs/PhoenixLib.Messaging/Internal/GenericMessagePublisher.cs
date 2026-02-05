using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus.MQTT;

namespace PhoenixLib.ServiceBus
{
    internal sealed class GenericMessagePublisher<T> : IMessagePublisher<T> where T : IMessage
    {
        private readonly IMessagingService _publisher;

        public GenericMessagePublisher(IMessagingService publisher) => _publisher = publisher;

        public async Task PublishAsync(T notification, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            await _publisher.SendAsync(notification);
        }
    }
}