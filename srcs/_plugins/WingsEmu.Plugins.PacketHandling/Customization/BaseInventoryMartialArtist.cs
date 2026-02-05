using System.Collections.Generic;
using WingsEmu.DTOs.Items;
using WingsEmu.Packets.Enums;
using static WingsEmu.Customization.NewCharCustomisation.BaseInventory;

namespace WingsEmu.Plugins.PacketHandling.Customization;

public class BaseInventoryMartialArtist
{
    public BaseInventoryMartialArtist() => Items =
    [
        // MartialArtist class starter equipment
        new StartupInventoryItem
        {
            Vnum = 4719, // Steel Fist
            Quantity = 1,
            InventoryType = InventoryType.EquippedItems,
            Upgrade = 8,
            Rare = 7,
            Options =
            [
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 1, Type = 16, Value = 35 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 2, Type = 1, Value = 110 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 24, Value = 100 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 26, Value = 12 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 4, Type = 2, Value = 15 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 5, Type = 32, Value = 15 }
            ]
        },
        new StartupInventoryItem
        {
            Vnum = 4760, // Three-Horse Bronze Token
            Quantity = 1,
            InventoryType = InventoryType.EquippedItems,
            Upgrade = 8,
            Rare = 7,
            Options =
            [
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 1, Type = 16, Value = 45 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 2, Type = 1, Value = 110 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 27, Value = 12 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 28, Value = 12 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 4, Type = 30, Value = 8 }
            ]
        },
        new StartupInventoryItem
        {
            Vnum = 4737, // Trainee Martial Artist's Uniform
            Quantity = 1,
            InventoryType = InventoryType.EquippedItems,
            Upgrade = 8,
            Rare = 7,
            Options =
            [
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.ARMOR_SHELL, Level = 1, Type = 73, Value = 6 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.ARMOR_SHELL, Level = 2, Type = 59, Value = 25 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.ARMOR_SHELL, Level = 3, Type = 51, Value = 170 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.ARMOR_SHELL, Level = 3, Type = 52, Value = 170 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.ARMOR_SHELL, Level = 4, Type = 67, Value = 25 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.ARMOR_SHELL, Level = 7, Type = 82, Value = 40 }
            ]
        },

        // MartialArtist unequipped gear
        new StartupInventoryItem
        {
            Vnum = 4730, // Ladine's Tear
            Quantity = 1,
            Slot = 0,
            InventoryType = InventoryType.Equipment,
            Upgrade = 8,
            Rare = 7,
            Options =
            [
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 1, Type = 16, Value = 35 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 2, Type = 1, Value = 110 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 24, Value = 100 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 26, Value = 12 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 4, Type = 2, Value = 15 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 5, Type = 32, Value = 15 }
            ]
        },
        new StartupInventoryItem
        {
            Vnum = 4764, // One-Horse Gold Token
            Quantity = 1,
            Slot = 1,
            InventoryType = InventoryType.Equipment,
            Upgrade = 8,
            Rare = 7,
            Options =
            [
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 1, Type = 16, Value = 45 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 2, Type = 1, Value = 110 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 27, Value = 12 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 28, Value = 12 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 4, Type = 30, Value = 8 }
            ]
        },
        new StartupInventoryItem
        {
            Vnum = 4747, // Desert Robbers' Armour
            Quantity = 1,
            Slot = 2,
            InventoryType = InventoryType.Equipment,
            Upgrade = 8,
            Rare = 7,
            Options =
            [
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.ARMOR_SHELL, Level = 1, Type = 73, Value = 6 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.ARMOR_SHELL, Level = 2, Type = 59, Value = 25 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.ARMOR_SHELL, Level = 3, Type = 51, Value = 170 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.ARMOR_SHELL, Level = 3, Type = 52, Value = 170 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.ARMOR_SHELL, Level = 4, Type = 67, Value = 25 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.ARMOR_SHELL, Level = 7, Type = 82, Value = 40 }
            ]
        },

        // Additional items
        new StartupInventoryItem { Vnum = 4503, Quantity = 1, Slot = 3, InventoryType = InventoryType.Equipment },
        new StartupInventoryItem { Vnum = 4504, Quantity = 1, Slot = 4, InventoryType = InventoryType.Equipment },


        // Specialists
        new StartupInventoryItem { Vnum = 4486, Quantity = 1, InventoryType = InventoryType.EquippedItems, Upgrade = 5, SpecialistOptions = [new ItemInstanceDTO { SpLevel = 50, Upgrade = 5 }] },
        new StartupInventoryItem { Vnum = 4485, Quantity = 1, Slot = 0, InventoryType = InventoryType.Specialist, Upgrade = 5, SpecialistOptions = [new ItemInstanceDTO { SpLevel = 50, Upgrade = 5 }] },
        new StartupInventoryItem { Vnum = 4437, Quantity = 1, Slot = 1, InventoryType = InventoryType.Specialist, Upgrade = 5, SpecialistOptions = [new ItemInstanceDTO { SpLevel = 50, Upgrade = 5 }] },
        new StartupInventoryItem { Vnum = 4532, Quantity = 1, Slot = 2, InventoryType = InventoryType.Specialist, Upgrade = 5, SpecialistOptions = [new ItemInstanceDTO { SpLevel = 50, Upgrade = 5 }] },
    ];

    public List<StartupInventoryItem> Items { get; set; }
}