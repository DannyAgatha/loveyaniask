using System.Threading;
using System.Threading.Tasks;

namespace PhoenixLib.ServiceBus
{
    public interface IMessageConsumer<in T>
    {
        Task HandleAsync(T notification, CancellationToken token);
    }
}