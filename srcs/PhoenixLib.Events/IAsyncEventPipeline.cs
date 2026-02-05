using System.Threading;
using System.Threading.Tasks;

namespace PhoenixLib.Events
{
    public interface IAsyncEventPipeline
    {
        /// <summary>
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        Task ProcessEventAsync(IAsyncEvent notification);

        /// <summary>
        ///     Asynchronously send a notification to handlers of type T
        /// </summary>
        /// <param name="notification">Notification object</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the publish operation.</returns>
        Task ProcessEventAsync<T>(T notification, CancellationToken cancellationToken = default) where T : IAsyncEvent;
    }
}