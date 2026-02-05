using System.Collections.Generic;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Configurations;

public class TrainerQuestConfiguration
{
    public List<QuestConfiguration> Quests { get; set; }
}

public class QuestConfiguration
{
    public PetTrainerMissionType MissionType { get; set; }

    public long AchievementNeeded { get; set; }
    
    public int RewardVnum { get; set; }
}