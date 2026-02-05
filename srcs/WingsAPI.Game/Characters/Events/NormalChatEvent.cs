using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class NormalChatEvent : PlayerEvent
{
    public string Message { get; init; }
}