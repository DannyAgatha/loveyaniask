// NosEmu
// 


using System.Threading;
using System.Threading.Tasks;

namespace PhoenixLib.Events
{
    /// <summary>
    ///     Defines a handler for any type of notification
    /// </summary>
    public interface IAsyncEventProcessor<in T>
    where T : IAsyncEvent
    {
        Task HandleAsync(T e, CancellationToken cancellation);
    }
}