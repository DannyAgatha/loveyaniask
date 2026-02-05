using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Raid
{
    [MessageType("raid.restriction-refresh")]
    public class RaidRestrictionRefreshMessage : IMessage
    {
    }
}