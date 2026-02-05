using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.FamilyImpl.Achievements;

[MessageType("family.achievements.increment")]
public class FamilyAchievementIncrementMessage : IMessage
{
    public long FamilyId { get; set; }
    public int AchievementId { get; set; }
    public int ValueToAdd { get; set; }
}