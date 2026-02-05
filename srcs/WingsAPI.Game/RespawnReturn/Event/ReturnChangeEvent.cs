using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.RespawnReturn.Event;

public class ReturnChangeEvent : PlayerEvent
{
    public int MapId { get; init; }
    public short MapX { get; init; }
    public short MapY { get; init; }

    public bool IsByGroup { get; init; }
}