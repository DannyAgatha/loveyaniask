using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Quests.Event;

public class QuestHarvestEvent : PlayerEvent
{
    public int ItemVnum { get; init; }
    public int NpcVnum { get; init; }
}