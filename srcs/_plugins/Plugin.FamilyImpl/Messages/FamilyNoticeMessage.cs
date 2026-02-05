using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.notice")]
    public class FamilyNoticeMessage : IMessage
    {
        public long FamilyId { get; set; }
        public string Message { get; set; }
    }
}