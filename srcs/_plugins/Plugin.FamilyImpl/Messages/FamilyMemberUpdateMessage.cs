using System.Collections.Generic;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsAPI.Data.Families;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.member.update")]
    public class FamilyMemberUpdateMessage : IMessage
    {
        public List<FamilyMembershipDto> UpdatedMembers { get; set; }

        public ChangedInfoMemberUpdate ChangedInfoMemberUpdate { get; set; }
    }

    public enum ChangedInfoMemberUpdate
    {
        None,
        Authority,
        Experience,
        DailyMessage
    }
}