using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class EnhancementSoulstoneHandler : IItemUsageByVnumHandler
{
    private static readonly HashSet<int> FirstFourSpecialistCards =
    [
        (short)ItemVnums.WARRIOR_SPECIALIST_CARD,
        (short)ItemVnums.NINJA_SPECIALIST_CARD,
        (short)ItemVnums.CRUSADER_SPECIALIST_CARD,
        (short)ItemVnums.BERSERKER_SPECIALIST_CARD,
        (short)ItemVnums.RANGER_SPECIALIST_CARD,
        (short)ItemVnums.ASSASSIN_SPECIALIST_CARD,
        (short)ItemVnums.DESTROYER_SPECIALIST_CARD,
        (short)ItemVnums.WILD_KEEPER_SPECIALIST_CARD,
        (short)ItemVnums.RED_MAGICIAN_SPECIALIST_CARD,
        (short)ItemVnums.HOLY_MAGE_SPECIALIST_CARD,
        (short)ItemVnums.BLUE_MAGICIAN_SPECIALIST_CARD,
        (short)ItemVnums.DARK_GUNNER_SPECIALIST_CARD,
        (short)ItemVnums.DRACONIC_FIST_SPECIALIST_CARD,
        (short)ItemVnums.MYSTIC_ARTS_SPECIALIST_CARD,
        (short)ItemVnums.MASTER_WOLF_SPECIALIST_CARD,
        (short)ItemVnums.DEMON_WARRIOR_SPECIALIST_CARD
    ];
    
    private static readonly HashSet<int> SecondFourSpecialistCards =
    [
        (short)ItemVnums.GLADIATOR_SPECIALIST_CARD,
        (short)ItemVnums.BATTLE_MONK_SPECIALIST_CARD,
        (short)ItemVnums.DEATH_REAPER_SPECIALIST_CARD,
        (short)ItemVnums.RENEGADE_SPECIALIST_CARD,
        (short)ItemVnums.FIRE_CANNONEER_SPECIALIST_CARD,
        (short)ItemVnums.SCOUT_SPECIALIST_CARD,
        (short)ItemVnums.DEMON_HUNTER_SPECIALIST_CARD,
        (short)ItemVnums.AVENGING_ANGEL_SPECIALIST_CARD,
        (short)ItemVnums.VOLCANO_SPECIALIST_CARD,
        (short)ItemVnums.TIDE_LORD_SPECIALIST_CARD,
        (short)ItemVnums.SEER_SPECIALIST_CARD,
        (short)ItemVnums.ARCHMAGE_SPECIALIST_CARD
    ];
    public long[] Vnums =>
    [
        (short)ItemVnums.BLUE_SOUL_STONE,
        (short)ItemVnums.SILVER_SOUL_STONE,
        (short)ItemVnums.GOLDEN_SOUL_STONE
    ];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (e.Packet is not { Length: >= 10 } || !short.TryParse(e.Packet[9], out short slot))
        {
            return;
        }

        if (!Enum.TryParse(e.Packet[8], out InventoryType inventoryType) || inventoryType != InventoryType.Equipment)
        {
            return;
        }

        InventoryItem specialist = session.PlayerEntity.GetItemBySlotAndType(slot, InventoryType.Equipment);
        if (specialist?.ItemInstance is not { } spInstance)
        {
            return;
        }

        if (!spInstance.IsSpecialistCard() || spInstance.IsLifestyleSpecialistCard() || spInstance.Rarity == -2)
        {
            return;
        }

        switch (e.Item.ItemInstance.ItemVNum)
        {
            case (short)ItemVnums.BLUE_SOUL_STONE:
                if (!FirstFourSpecialistCards.Contains(spInstance.ItemVNum) || spInstance.Upgrade >= 9)
                {
                    return;
                }

                spInstance.Upgrade = 9;
                
                if (spInstance.SpLevel < 41)
                {
                    spInstance.SpLevel = 41;
                }
                break;
            
            case (short)ItemVnums.SILVER_SOUL_STONE:
                if (!SecondFourSpecialistCards.Contains(spInstance.ItemVNum) || spInstance.Upgrade >= 9)
                {
                    return;
                }

                spInstance.Upgrade = 9;
                
                if (spInstance.SpLevel < 41)
                {
                    spInstance.SpLevel = 41;
                }
                break;
            
            case (short)ItemVnums.GOLDEN_SOUL_STONE:
                if (spInstance.Upgrade >= 9)
                {
                    return;
                }

                spInstance.Upgrade = 9;
                
                if (spInstance.SpLevel < 41)
                {
                    spInstance.SpLevel = 41;
                }
                break;
        }

        await session.RemoveItemFromInventory(item: e.Item);
    }
}