using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Quests.Event;

public class QuestDailyRefreshEvent : PlayerEvent
{
    public bool Force { get; init; }
}