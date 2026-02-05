using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.InterChannel;

public class FamilyChatMessageEvent : PlayerEvent
{
    public FamilyChatMessageEvent(string message) => Message = message;

    public string Message { get; }
}