using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsAPI.Data.Families;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.member.added")]
    public class FamilyMemberAddedMessage : IMessage
    {
        public FamilyMembershipDto AddedMember { get; set; }

        public string Nickname { get; set; }

        public long SenderId { get; set; }
    }
}