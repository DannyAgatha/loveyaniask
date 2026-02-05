using System;

namespace PhoenixLib.ServiceBus.Routing
{
    public class RoutingInformation : IRoutingInformation
    {
        public RoutingInformation(Type type, string topic, string eventType)
        {
            ObjectType = type;
            Topic = topic;
            EventType = eventType;
        }

        public Type ObjectType { get; }
        public string Topic { get; }
        public string EventType { get; }
    }
}