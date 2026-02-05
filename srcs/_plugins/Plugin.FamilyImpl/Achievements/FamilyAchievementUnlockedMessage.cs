using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.FamilyImpl.Achievements
{
    [MessageType("family.achievements.unlocked")]
    public class FamilyAchievementUnlockedMessage : IMessage
    {
        public long FamilyId { get; set; }
        public int AchievementId { get; set; }
    }
}