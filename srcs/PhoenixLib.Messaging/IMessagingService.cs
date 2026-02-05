using System;
using System.Threading.Tasks;

namespace PhoenixLib.ServiceBus.MQTT
{
    public interface IMessagingService : IAsyncDisposable
    {
        /// <summary>
        ///     Should not be exposed but permits to send IMessage on the MessageQueue
        /// </summary>
        /// <param name="eventToSend"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task SendAsync<T>(T eventToSend) where T : IMessage;

        Task StartAsync();
    }
}