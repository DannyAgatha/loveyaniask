using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Act7.Tattoos;

public class TattooCraftEvent : PlayerEvent
{
    public TattooCraftEvent(InventoryItem pattern) => Pattern = pattern;
    public InventoryItem Pattern { get; set; }
}