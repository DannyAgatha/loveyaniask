using System;

namespace PhoenixLib.ServiceBus.Routing
{
    public interface IRoutingInformation
    {
        Type ObjectType { get; }
        string Topic { get; }
        string EventType { get; }
    }
}