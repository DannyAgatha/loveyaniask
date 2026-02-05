using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.member.today")]
    public class FamilyMemberTodayMessage : IMessage
    {
        public long CharacterId { get; set; }
        public string Message { get; set; }
    }
}