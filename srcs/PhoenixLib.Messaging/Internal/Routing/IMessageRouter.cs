using System;

namespace PhoenixLib.ServiceBus.Routing
{
    public interface IMessageRouter
    {
        /// <summary>
        ///     Get the routing informations from the router
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IRoutingInformation GetRoutingInformation<T>();

        /// <summary>
        ///     Used for runtime possibilities
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IRoutingInformation GetRoutingInformation(Type type);

        /// <summary>
        ///     Gets the routing information
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IRoutingInformation GetRoutingInformation(string type);
    }
}