using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyMessageSentEvent : PlayerEvent
{
    public string Message { get; init; }
    public FamilyMessageType MessageType { get; init; }
}