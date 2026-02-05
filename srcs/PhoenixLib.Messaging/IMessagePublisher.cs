using System.Threading;
using System.Threading.Tasks;

namespace PhoenixLib.ServiceBus
{
    public interface IMessagePublisher<in T>
    where T : IMessage
    {
        /// <summary>
        ///     Publishes the given event
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task PublishAsync(T notification, CancellationToken token = default);
    }
}