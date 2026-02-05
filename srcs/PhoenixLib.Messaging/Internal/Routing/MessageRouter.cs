using System;
using System.Collections.Concurrent;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.Internal.Routing;

namespace PhoenixLib.ServiceBus.Routing
{
    public class MessageRouter : IMessageRouter
    {
        private static readonly ConcurrentDictionary<Type, IRoutingInformation> _infos = new();
        private static readonly ConcurrentDictionary<string, IRoutingInformation> _infosByCloudEventType = new();
        private readonly IRoutingInformationFactory _routingInformationFactory;

        public MessageRouter(IRoutingInformationFactory routingInformationFactory) =>
            _routingInformationFactory = routingInformationFactory;

        public IRoutingInformation GetRoutingInformation<T>() => GetRoutingInformation(typeof(T));

        public IRoutingInformation GetRoutingInformation(Type type) => _infos.GetOrAdd(type, Register);

        public IRoutingInformation GetRoutingInformation(string type) =>
            _infosByCloudEventType.TryGetValue(type, out IRoutingInformation value) ? value : throw new ArgumentException($"type: {type} is not registered");

        private IRoutingInformation Register(Type type)
        {
            Log.Debug($"Register: {type}");
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsGenericType)
            {
                throw new ArgumentException("Generics are not yet supported");
            }

            MessageTypeAttribute eventType = type.GetMessageAttributes();
            if (string.IsNullOrEmpty(eventType.EventType))
            {
                throw new ArgumentException($"{type} misses the attribute EventTypeAttribute on the class");
            }

            IRoutingInformation routingInfos = _routingInformationFactory.Create(type, eventType.EventType.Replace('.', '/'), eventType.EventType);
            Register(type, routingInfos);
            return routingInfos;
        }

        private static void Register(Type type, IRoutingInformation routingInformation)
        {
            _infos[type] = routingInformation;
            _infosByCloudEventType[routingInformation.EventType] = routingInformation;
        }
    }
}
