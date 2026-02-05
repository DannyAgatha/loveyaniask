using System;

namespace PhoenixLib.ServiceBus
{
    internal class GenericSubscribedMessage<T> : ISubscribedMessage
    where T : IMessage
    {
        public Type Type => typeof(T);
    }
}