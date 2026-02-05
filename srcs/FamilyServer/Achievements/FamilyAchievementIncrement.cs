using PhoenixLib.Events;

namespace FamilyServer.Achievements
{
    public class FamilyAchievementIncrement : IAsyncEvent
    {
        public long FamilyId { get; set; }
        public int AchievementId { get; set; }
        public int ValueToAdd { get; set; }
    }
}