using System;

namespace PhoenixLib.ServiceBus.Routing
{
    public class MessageTypeAttribute : Attribute
    {
        public MessageTypeAttribute(string eventType) => EventType = eventType;

        public string EventType { get; set; }
    }
}