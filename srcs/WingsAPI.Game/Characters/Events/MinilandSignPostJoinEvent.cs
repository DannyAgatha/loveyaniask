using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class MinilandSignPostJoinEvent : PlayerEvent
{
    public long PlayerId { get; init; }
}