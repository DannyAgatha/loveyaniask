using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.disband")]
    public class FamilyDisbandMessage : IMessage
    {
        public long FamilyId { get; init; }
    }
}