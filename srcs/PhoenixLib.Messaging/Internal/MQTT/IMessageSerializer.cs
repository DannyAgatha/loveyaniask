using System;
using MQTTnet;

namespace PhoenixLib.ServiceBus.Protocol
{
    internal interface IMessageSerializer
    {
        MqttApplicationMessage ToMessage<T>(T packet) where T : IMessage;
        (object obj, Type objType) FromMessage(MqttApplicationMessage messageApplicationMessage);
    }
}