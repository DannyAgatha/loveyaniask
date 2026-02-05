// NosEmu
// 


using WingsEmu.DTOs.Quicklist;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Quicklist;

public class QuicklistAddEvent : PlayerEvent
{
    public short Tab { get; init; }
    public short Slot { get; init; }
    public QuicklistType Type { get; init; }
    public short DestinationType { get; init; }
    public short DestinationSlotOrVnum { get; init; }
}