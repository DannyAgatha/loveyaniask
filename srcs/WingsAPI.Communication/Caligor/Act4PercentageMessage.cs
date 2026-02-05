using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Caligor;

[MessageType("act4.percentage")]
public class Act4PercentageMessage : IMessage
{
}