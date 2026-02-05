using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.RespawnReturn.Event;

public class RespawnChangeEvent : PlayerEvent
{
    public int MapId { get; init; }
}