// NosEmu
// 


using System.Collections.Generic;
using WingsEmu.DTOs.Items;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Customization.NewCharCustomisation;

public class BaseInventory
{
    public BaseInventory() => Items = new List<StartupInventoryItem>
    {
        
    };

    public List<StartupInventoryItem> Items { get; set; }

    public class StartupInventoryItem
    {
        public short Vnum { get; set; }
        public short Quantity { get; set; }
        public short Slot { get; set; }
        public InventoryType InventoryType { get; set; }
        public byte Upgrade { get; set; }
        public byte Rare { get; set; }
        public List<EquipmentOptionDTO> Options { get; set; } = [];
        public List<ItemInstanceDTO> SpecialistOptions { get; set; } = [];
    }
}