using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Quests.Event;

public class QuestCompletedEvent : PlayerEvent
{
    public QuestCompletedEvent(CharacterQuest characterQuest, bool claimReward = false, bool refreshProgress = true, bool giveNextQuest = true, bool ignoreNotCompletedQuest = false)
    {
        CharacterQuest = characterQuest;
        ClaimReward = claimReward;
        RefreshProgress = refreshProgress;
        GiveNextQuest = giveNextQuest;
        IgnoreNotCompletedQuest = ignoreNotCompletedQuest;
    }

    public CharacterQuest CharacterQuest { get; }
    public bool ClaimReward { get; }
    public bool GiveNextQuest { get; }
    public bool RefreshProgress { get; }
    public bool IgnoreNotCompletedQuest { get; }
}