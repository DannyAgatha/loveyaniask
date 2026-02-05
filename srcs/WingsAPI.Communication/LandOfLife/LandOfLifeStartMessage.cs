using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.LandOfLife;

[MessageType("lol.start")]
public class LandOfLifeStartMessage : IMessage
{
}