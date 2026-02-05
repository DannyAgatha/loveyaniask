using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.InterChannel;

public class ChatShoutAdminEvent : PlayerEvent
{
    public string Message { get; init; }
}