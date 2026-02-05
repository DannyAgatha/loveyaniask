using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Mates.PartnerFusion;

public class PartnerFusionEvent : PlayerEvent
{
    public PartnerFusionEvent(InventoryItem psp, InventoryItem material)
    {
        Psp = psp;
        Material = material;
    }

    public InventoryItem Psp { get; }
    public InventoryItem Material { get; }
}