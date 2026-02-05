using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.created")]
    public class FamilyCreatedMessage : IMessage
    {
        public string FamilyName { get; set; }
    }
}