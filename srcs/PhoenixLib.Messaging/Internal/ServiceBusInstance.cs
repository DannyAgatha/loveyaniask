using System;

namespace PhoenixLib.ServiceBus
{
    internal class ServiceBusInstance : IServiceBusInstance
    {
        private static readonly Guid _id = Guid.NewGuid();
        public Guid Id => _id;
    }
}