namespace Plugin.FamilyImpl.Achievements;

public interface IFamilyAchievementManager
{
    void IncrementFamilyAchievement(long familyId, int achievementId, int counterToAdd);
    void IncrementFamilyAchievement(long familyId, int achievementId);
}