using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Quicklist;

public class QuicklistRemoveEvent : PlayerEvent
{
    public short Tab { get; init; }
    public short Slot { get; init; }
}