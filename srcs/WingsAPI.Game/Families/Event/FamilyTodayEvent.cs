using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyTodayEvent : PlayerEvent
{
    public FamilyTodayEvent(string message) => Message = message;

    public string Message { get; }
}