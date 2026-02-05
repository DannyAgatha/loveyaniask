using System;

namespace PhoenixLib.ServiceBus.Routing
{
    public interface IRoutingInformationFactory
    {
        IRoutingInformation Create(Type type, string topic, string eventType);
    }
}