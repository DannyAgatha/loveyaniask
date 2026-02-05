using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Item
{
    [MessageType("item.restriction-refresh")]
    public class ItemRestrictionRefreshMessage : IMessage
    {
    
    }
}