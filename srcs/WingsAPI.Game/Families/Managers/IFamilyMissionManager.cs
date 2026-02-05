namespace Plugin.FamilyImpl.Achievements;

public interface IFamilyMissionManager
{
    void IncrementFamilyMission(long familyId, long? playerId, int missionId, int counterToAdd);
    void IncrementFamilyMission(long familyId, long? playerId, int missionId);
    void IncrementFamilyMission(long familyId, int missionId);
}