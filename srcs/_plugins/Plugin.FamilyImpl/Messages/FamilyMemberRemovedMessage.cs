using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.member.removed")]
    public class FamilyMemberRemovedMessage : IMessage
    {
        public long CharacterId { get; set; }

        public long FamilyId { get; set; }
    }
}