// NosEmu
// 


using System;
using System.Reflection;
using PhoenixLib.ServiceBus.Routing;

namespace PhoenixLib.ServiceBus.Internal.Routing
{
    internal static class MessageExtensions
    {
        internal static MessageTypeAttribute GetMessageAttributes(this Type type)
        {
            MessageTypeAttribute messageTypeAttribute = type.GetCustomAttribute<MessageTypeAttribute>();
            if (messageTypeAttribute == null)
            {
                throw new ArgumentException($"{type} misses the attribute EventTypeAttribute on the class");
            }

            return messageTypeAttribute;
        }
    }
}