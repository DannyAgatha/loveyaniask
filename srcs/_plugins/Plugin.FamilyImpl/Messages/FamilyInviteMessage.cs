using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.member.invite")]
    public class FamilyInviteMessage : IMessage
    {
        public string ReceiverNickname { get; set; }

        public long SenderCharacterId { get; set; }

        public long FamilyId { get; set; }

        public string FamilyName { get; set; }
    }
}