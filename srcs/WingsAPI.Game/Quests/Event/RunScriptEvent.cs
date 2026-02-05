using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Quests.Event;

public class RunScriptEvent : PlayerEvent
{
    public int RunId { get; init; }
}