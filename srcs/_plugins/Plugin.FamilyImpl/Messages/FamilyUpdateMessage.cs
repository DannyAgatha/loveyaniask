using System.Collections.Generic;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsAPI.Data.Families;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.update")]
    public class FamilyUpdateMessage : IMessage
    {
        public IReadOnlyCollection<FamilyDTO> Families { get; set; }

        public ChangedInfoFamilyUpdate ChangedInfoFamilyUpdate { get; set; }
    }

    public enum ChangedInfoFamilyUpdate
    {
        None,
        Experience,
        Notice,
        HeadSex,
        Settings,
        Upgrades,
        AchievementsAndMissions
    }
}