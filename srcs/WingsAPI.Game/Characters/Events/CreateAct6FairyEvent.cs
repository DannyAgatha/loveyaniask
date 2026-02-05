using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Characters.Events
{
    public class CreateAct6FairyEvent : PlayerEvent
    {
        public InventoryItem Inv { get; set; }

        public byte FairyType { get; set; }
    }
}