using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Quests.Event;

public class QuestItemPickUpEvent : PlayerEvent
{
    public int ItemVnum { get; init; }
    public int Amount { get; init; }
    public bool SendMessage { get; init; }
}