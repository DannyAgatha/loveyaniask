using System;

namespace PhoenixLib.ServiceBus.Routing
{
    internal class RoutingInformationFactory : IRoutingInformationFactory
    {
        public IRoutingInformation Create(Type type, string topic, string eventType) => new RoutingInformation(type, topic, eventType);
    }
}