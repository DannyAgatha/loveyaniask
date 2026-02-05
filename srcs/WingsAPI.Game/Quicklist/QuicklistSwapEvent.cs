using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Quicklist;

public class QuicklistSwapEvent : PlayerEvent
{
    public short Tab { get; init; }
    public short FromSlot { get; init; }
    public short ToSlot { get; init; }
}