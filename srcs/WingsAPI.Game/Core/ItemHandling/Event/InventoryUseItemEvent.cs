using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game._ItemUsage.Event;

public class InventoryUseItemEvent : PlayerEvent
{
    public InventoryItem Item { get; set; }

    public byte Option { get; set; }

    public string[] Packet { get; set; }
}