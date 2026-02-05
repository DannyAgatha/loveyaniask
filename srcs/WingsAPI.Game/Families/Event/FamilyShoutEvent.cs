using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyShoutEvent : PlayerEvent
{
    public FamilyShoutEvent(string message) => Message = message;

    public string Message { get; }
}