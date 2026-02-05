using System.Collections.Generic;
using WingsEmu.DTOs.Items;
using WingsEmu.Packets.Enums;
using static WingsEmu.Customization.NewCharCustomisation.BaseInventory;

namespace WingsEmu.Plugins.PacketHandling.Customization;

public class BaseInventoryMagician
{
    public BaseInventoryMagician() => Items =
    [
        // Equipment Items
        new StartupInventoryItem
        {
            Vnum = 152,
            Quantity = 1,
            InventoryType = InventoryType.EquippedItems,
            Upgrade = 10,
            Rare = 7,
            Options =
            [
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 1, Type = 17, Value = 1 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 2, Type = 20, Value = 100 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 1, Value = 160 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 26, Value = 12 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 4, Type = 2, Value = 15 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 5, Type = 32, Value = 15 }
            ]
        },
        new StartupInventoryItem
        {
            Vnum = 92,
            Quantity = 1,
            InventoryType = InventoryType.EquippedItems,
            Upgrade = 10,
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
            Vnum = 271,
            Quantity = 1,
            InventoryType = InventoryType.EquippedItems,
            Upgrade = 10,
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

        // Equipments not equipped

        new StartupInventoryItem
        {
            Vnum = 155,
            Quantity = 1,
            Slot = 0,
            InventoryType = InventoryType.Equipment,
            Upgrade = 10,
            Rare = 7,
            Options =
            [
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 1, Type = 17, Value = 1 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 2, Type = 20, Value = 100 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 1, Value = 160 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 26, Value = 12 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 4, Type = 2, Value = 15 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 5, Type = 32, Value = 15 }
            ]
        },
        new StartupInventoryItem
        {
            Vnum = 294,
            Quantity = 1,
            Slot = 1,
            InventoryType = InventoryType.Equipment,
            Upgrade = 10,
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
            Vnum = 989,
            Quantity = 1,
            Slot = 2,
            InventoryType = InventoryType.Equipment,
            Upgrade = 10,
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
        new StartupInventoryItem
        {
            Vnum = 407,
            Quantity = 1,
            Slot = 3,
            InventoryType = InventoryType.Equipment,
            Upgrade = 10,
            Rare = 7,
            Options =
            [
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 1, Type = 17, Value = 1 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 2, Type = 20, Value = 100 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 1, Value = 160 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 26, Value = 12 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 4, Type = 2, Value = 15 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 5, Type = 32, Value = 15 }
            ]
        },
        new StartupInventoryItem
        {
            Vnum = 765,
            Quantity = 1,
            Slot = 4,
            InventoryType = InventoryType.Equipment,
            Upgrade = 10,
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
            Vnum = 411,
            Quantity = 1,
            Slot = 5,
            InventoryType = InventoryType.Equipment,
            Upgrade = 10,
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
        new StartupInventoryItem
        {
            Vnum = 4005,
            Quantity = 1,
            Slot = 6,
            InventoryType = InventoryType.Equipment,
            Upgrade = 10,
            Rare = 7,
            Options =
            [
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 1, Type = 17, Value = 1 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 2, Type = 20, Value = 100 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 1, Value = 160 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 3, Type = 26, Value = 12 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 4, Type = 2, Value = 15 },
                new EquipmentOptionDTO { EquipmentOptionType = EquipmentOptionType.WEAPON_SHELL, Level = 5, Type = 32, Value = 15 }
            ]
        },
        new StartupInventoryItem
        {
            Vnum = 4011,
            Quantity = 1,
            Slot = 7,
            InventoryType = InventoryType.Equipment,
            Upgrade = 10,
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
            Vnum = 4019,
            Quantity = 1,
            Slot = 8,
            InventoryType = InventoryType.Equipment,
            Upgrade = 10,
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


        // Additional Items
        new StartupInventoryItem { Vnum = 4503, Quantity = 1, Slot = 9, InventoryType = InventoryType.Equipment },
        new StartupInventoryItem { Vnum = 4504, Quantity = 1, Slot = 10, InventoryType = InventoryType.Equipment },

        // Specialists
        new StartupInventoryItem
        {
            Vnum = 905,
            Quantity = 1,
            InventoryType = InventoryType.EquippedItems,
            Upgrade = 5,
            SpecialistOptions =
            [
                new ItemInstanceDTO
                {
                    SpLevel = 50,
                    Upgrade = 5,
                    SpStoneUpgrade = 0,
                    SpDamage = 0,
                    SpDefence = 0,
                    SpElement = 0,
                    SpHP = 0,
                    SpFire = 0,
                    SpWater = 0,
                    SpLight = 0,
                    SpDark = 0
                }
            ]
        },
        new StartupInventoryItem
        {
            Vnum = 906,
            Quantity = 1,
            Slot = 0,
            InventoryType = InventoryType.Specialist,
            Upgrade = 5,
            SpecialistOptions =
            [
                new ItemInstanceDTO
                {
                    SpLevel = 50,
                    Upgrade = 5,
                    SpStoneUpgrade = 0,
                    SpDamage = 0,
                    SpDefence = 0,
                    SpElement = 0,
                    SpHP = 0,
                    SpFire = 0,
                    SpWater = 0,
                    SpLight = 0,
                    SpDark = 0
                }
            ]
        },
        new StartupInventoryItem
        {
            Vnum = 913,
            Quantity = 1,
            Slot = 1,
            InventoryType = InventoryType.Specialist,
            Upgrade = 5,
            SpecialistOptions =
            [
                new ItemInstanceDTO
                {
                    SpLevel = 50,
                    Upgrade = 5,
                    SpStoneUpgrade = 0,
                    SpDamage = 0,
                    SpDefence = 0,
                    SpElement = 0,
                    SpHP = 0,
                    SpFire = 0,
                    SpWater = 0,
                    SpLight = 0,
                    SpDark = 0
                }
            ]
        },
        new StartupInventoryItem
        {
            Vnum = 914,
            Quantity = 1,
            Slot = 2,
            InventoryType = InventoryType.Specialist,
            Upgrade = 5,
            SpecialistOptions =
            [
                new ItemInstanceDTO
                {
                    SpLevel = 50,
                    Upgrade = 5,
                    SpStoneUpgrade = 0,
                    SpDamage = 0,
                    SpDefence = 0,
                    SpElement = 0,
                    SpHP = 0,
                    SpFire = 0,
                    SpWater = 0,
                    SpLight = 0,
                    SpDark = 0
                }
            ]
        }
    ];

    public List<StartupInventoryItem> Items { get; set; }
}