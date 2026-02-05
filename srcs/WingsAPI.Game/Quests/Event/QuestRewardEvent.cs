using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Quests.Event;

public class QuestRewardEvent : PlayerEvent
{
    public int QuestId { get; init; }
    public bool ClaimReward { get; init; }
}