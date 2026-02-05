using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.LandOfLife;

[MessageType("land.of.life.restriction-refresh")]
public class LandOfLifeRestrictionRefreshMessage : IMessage
{
}