using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Exchange.Event;

public class ExchangeCloseEvent : PlayerEvent
{
    public ExcCloseType Type { get; set; }
}