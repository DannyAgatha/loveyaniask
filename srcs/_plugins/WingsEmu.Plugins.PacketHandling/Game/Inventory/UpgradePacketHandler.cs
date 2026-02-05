using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quicklist;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.PartnerFusion;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Language;
using NosEmu.Plugins.BasicImplementations.Event.Characters;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game.Act7.CarvedRunes;
using WingsEmu.Game.Act7.Tattoos;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class UpgradePacketHandler : GenericGamePacketHandlerBase<UpgradePacket>
{
    private readonly IGameLanguageService _language;
    private readonly ISkillsManager _skillManager;
    private readonly PetMaxLevelConfiguration _configuration;

    public UpgradePacketHandler(IGameLanguageService language, ISkillsManager skillsManagers, PetMaxLevelConfiguration configuration)
    {
        _language = language;
        _skillManager = skillsManagers;
        _configuration = configuration;
    }

    private bool CanBeUsed(InventoryItem item)
    {
        if (!item.ItemInstance.GameItem.IsDroppable && !item.ItemInstance.GameItem.IsTradable && !item.ItemInstance.GameItem.IsSoldable)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    protected override async Task HandlePacketAsync(IClientSession session, UpgradePacket upgradePacket)
    {
        if (session.IsActionForbidden() || session.PlayerEntity.IsInExchange() || session.PlayerEntity.HasShopOpened || session.PlayerEntity.IsShopping || session.PlayerEntity.IsWarehouseOpen
            || session.PlayerEntity.IsPartnerWarehouseOpen || session.PlayerEntity.IsFamilyWarehouseOpen || session.PlayerEntity.HasNosBazaarOpen)
        {
            return;
        }

        if (session.PlayerEntity.LastItemUpgrade.AddSeconds(4) > DateTime.UtcNow)
        {
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            return;
        }

        var inventoryType = (InventoryType)upgradePacket.Data;
        if (!Enum.TryParse(upgradePacket.UpgradeType.ToString(), out UpgradePacketType upType))
        {
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            return;
        }

        short slot = upgradePacket.Data2;
        InventoryItem inventory;
        InventoryItem specialist2 = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);
        session.PlayerEntity.LastItemUpgrade = DateTime.UtcNow;
        switch (upType)
        {
            case UpgradePacketType.FREE_CHICKEN_UPGRADE:
                // chicken SP
                inventory = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);

                if (inventory?.ItemInstance.ItemVNum != (short)ItemVnums.CHICKEN_SP)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (specialist2 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (specialist2.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Sp)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (specialist2.ItemInstance.Rarity == -2)
                {
                    session.SendMsg(_language.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_CANT_UPGRADE_DESTROYED_SP, session.UserLanguage), MsgMessageType.Middle);
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                await session.EmitEventAsync(new SpUpgradeEvent(UpgradeProtection.Protected, specialist2, true));
                break;
            case UpgradePacketType.FREE_PAJAMA_UPGRADE:
                // sp pyj
                inventory = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);

                if (inventory?.ItemInstance.ItemVNum != (short)ItemVnums.PYJAMA_SP)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (specialist2 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (specialist2.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Sp)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (specialist2.ItemInstance.Rarity == -2)
                {
                    session.SendMsg(_language.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_CANT_UPGRADE_DESTROYED_SP, session.UserLanguage), MsgMessageType.Middle);
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                await session.EmitEventAsync(new SpUpgradeEvent(UpgradeProtection.Protected, specialist2, true));
                break;
            case UpgradePacketType.FREE_PIRATE_UPGRADE:
                inventory = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);

                if (inventory?.ItemInstance.ItemVNum != (short)ItemVnums.PIRATE_SP)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (specialist2 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (specialist2.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Sp)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (specialist2.ItemInstance.Rarity == -2)
                {
                    session.SendMsg(_language.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_CANT_UPGRADE_DESTROYED_SP, session.UserLanguage), MsgMessageType.Middle);
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                await session.EmitEventAsync(new SpUpgradeEvent(UpgradeProtection.Protected, specialist2, true));
                break;
            case UpgradePacketType.PLAYER_ITEM_TO_PARTNER:
                await session.EmitEventAsync(new PlayerItemToPartnerItemEvent(slot, inventoryType));
                break;
            case UpgradePacketType.ITEM_UPGRADE:
                inventory = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);

                if (inventory == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Armor && inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.MainWeapon &&
                    inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.SecondaryWeapon)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                GameItemInstance amulet1 = session.PlayerEntity.Amulet;
                FixedUpMode hasAmulet1 = amulet1?.GameItem.Effect == 793 ? FixedUpMode.HasAmulet : FixedUpMode.None;

                await session.EmitEventAsync(new UpgradeItemEvent
                {
                    Inv = inventory,
                    Mode = UpgradeMode.Normal,
                    Protection = UpgradeProtection.None,
                    HasAmulet = hasAmulet1
                });

                break;
            case UpgradePacketType.CELLON_UPGRADE:
                if (upgradePacket.Data3 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                inventory = session.PlayerEntity.GetItemBySlotAndType((byte)upgradePacket.Data3.Value, (InventoryType)upgradePacket.Data);

                if (inventory == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Necklace
                    && inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Ring
                    && inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Bracelet)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (upgradePacket.Data6 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (upgradePacket.Data5 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (upgradePacket.Data5.Value != (short)InventoryType.Main)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                InventoryItem cellon = session.PlayerEntity.GetItemBySlotAndType(upgradePacket.Data6.Value, (InventoryType)upgradePacket.Data5.Value);
                if (cellon == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (cellon.ItemInstance.GameItem.Effect != 100)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (inventory.ItemInstance.Type != ItemInstanceType.WearableInstance)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                await session.EmitEventAsync(new CellonUpgradeEvent(cellon, inventory.ItemInstance));
                break;
            case UpgradePacketType.ITEM_RARITY:
                inventory = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);

                if (inventory == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Armor && inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.MainWeapon &&
                    inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.SecondaryWeapon)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                InventoryItem amulet7 = session.PlayerEntity.GetInventoryItemFromEquipmentSlot(EquipmentType.Amulet);

                await session.EmitRarifyEvent(inventory, amulet7);
                session.SendShopEndPacket(ShopEndType.Npc);
                break;
            case UpgradePacketType.ITEM_SUM:
                inventory = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);
                if (upgradePacket.Data3 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (upgradePacket.Data4 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (inventory == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                InventoryItem inventory2 = session.PlayerEntity.GetItemBySlotAndType((byte)upgradePacket.Data4, (InventoryType)upgradePacket.Data3);
                if (inventory2 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                await session.EmitEventAsync(new ItemSumEvent(inventory, inventory2));
                break;

            case UpgradePacketType.SP_UPGRADE:
                InventoryItem specialist = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);

                if (specialist == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (specialist.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Sp)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                await session.EmitEventAsync(new SpUpgradeEvent(UpgradeProtection.None, specialist));
                break;

            case UpgradePacketType.ITEM_UPGRADE_SCROLL:
                inventory = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);

                if (inventory == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                GameItemInstance amulet9 = session.PlayerEntity.Amulet;
                FixedUpMode hasAmulet9 = amulet9?.GameItem.Effect == 793 ? FixedUpMode.HasAmulet : FixedUpMode.None;

                if (inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Armor
                    && inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.MainWeapon
                    && inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.SecondaryWeapon)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                await session.EmitEventAsync(new UpgradeItemEvent
                {
                    Inv = inventory,
                    Mode = UpgradeMode.Normal,
                    Protection = UpgradeProtection.Protected,
                    HasAmulet = hasAmulet9
                });
                break;

            case UpgradePacketType.ITEM_RARITY_SCROLL:
                inventory = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);

                if (inventory == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Armor
                    && inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.MainWeapon
                    && inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.SecondaryWeapon)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                await session.EmitRarifyEvent(inventory, null, isScroll: true);
                break;

            case UpgradePacketType.SP_UPGRADE_SCROLL_BLUE:
            case UpgradePacketType.SP_UPGRADE_SCROLL_RED:
            case UpgradePacketType.SP_UPGRADE_SCROLL_DRAGON:
                specialist = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);

                if (specialist == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (specialist.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Sp)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                await session.EmitEventAsync(new SpUpgradeEvent(UpgradeProtection.Protected, specialist, isPremium: upgradePacket.Data4 == 2));
                break;

            case UpgradePacketType.SP_PERFECTION:
                specialist = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);
                if (specialist == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (specialist.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Sp)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (specialist.ItemInstance.Rarity == -2)
                {
                    session.SendMsg(_language.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_CANT_UPGRADE_DESTROYED_SP, session.UserLanguage), MsgMessageType.Middle);
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                await session.EmitEventAsync(new SpPerfectEvent(specialist));
                break;

            case UpgradePacketType.ITEM_UPGRADE_GOLD_SCROLL:
                inventory = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);
                if (inventory == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                GameItemInstance amulet43 = session.PlayerEntity.Amulet;
                FixedUpMode hasAmulet43 = amulet43?.GameItem.Effect == 793 ? FixedUpMode.HasAmulet : FixedUpMode.None;

                if (inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Armor
                    && inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.MainWeapon
                    && inventory.ItemInstance.GameItem.EquipmentSlot != EquipmentType.SecondaryWeapon)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                await session.EmitEventAsync(new UpgradeItemEvent
                {
                    Inv = inventory,
                    Mode = UpgradeMode.Reduced,
                    Protection = UpgradeProtection.Protected,
                    HasAmulet = hasAmulet43
                });
                break;
            
            case UpgradePacketType.PARTNER_FUSION:
                if (!upgradePacket.Data4.HasValue)
                {
                    return;
                }
                inventory = session.PlayerEntity.GetItemBySlotAndType(upgradePacket.Data2, InventoryType.Equipment);
                inventory2 = session.PlayerEntity.GetItemBySlotAndType((byte)upgradePacket.Data4, InventoryType.Equipment);
                await session.EmitEventAsync(new PartnerFusionEvent(inventory, inventory2));
                break;

            case UpgradePacketType.CREATE_ZENAS_FAIRY:
            case UpgradePacketType.CREATE_ERENIA_FAIRY:
            case UpgradePacketType.CREATE_FERNON_FAIRY:
                inventory = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);
                if (inventory == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                await session.EmitEventAsync(new CreateAct6FairyEvent
                {
                    Inv = inventory,
                    FairyType = (byte)(upType - 50)
                });
                break;
            
            case UpgradePacketType.COSTUME_FUSION:
                InventoryItem leftItem = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);
                if (leftItem == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    session.SendShopEndPacket(ShopEndType.Item);
                    return;
                }

                if (upgradePacket.Data4 is null || upgradePacket.Data3 is null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    session.SendShopEndPacket(ShopEndType.Item);
                    return;
                }
                
                InventoryItem rightItem = session.PlayerEntity.GetItemBySlotAndType((byte)upgradePacket.Data4, (InventoryType)upgradePacket.Data3);
                if (rightItem == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    session.SendShopEndPacket(ShopEndType.Item);
                    return;
                }
                
                await session.EmitEventAsync(new CostumeFusionEvent
                {
                    LeftItem = leftItem,
                    RightItem = rightItem
                });
                break;
            
            case UpgradePacketType.UPGRADE_PET:
                IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMates().FirstOrDefault(s => s.PetSlot == upgradePacket.Data);

                if (mateEntity == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                MaxPetLevelConfiguration infos = _configuration.Configurations.FirstOrDefault(s => s.Stars == mateEntity.Stars);

                if (infos == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (mateEntity.HeroLevel < infos.MaxLevel)
                {
                    session.SendMsgi(MessageType.Default, Game18NConstString.CanOnlyUpgradeAtMaximumTrainingLevel);
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                await session.EmitEventAsync(new UpgradePetEvent
                {
                    Mate = mateEntity
                });
                break;
            
             case UpgradePacketType.CRAFT_TATTOO:
                
                inventory = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);
                
                if (inventory == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }
                
                await session.EmitEventAsync(new TattooCraftEvent(inventory));
                break;

            case UpgradePacketType.TATTOO:
            {
                IBattleEntitySkill skill = session.PlayerEntity.Skills.FirstOrDefault(s => s.Skill.Id == upgradePacket.Data2);
                
                if (skill == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                switch (upgradePacket.Data)
                {
                    case 1:
                        await session.EmitEventAsync(new UpgradeTattooEvent(TattooUpgradeProtection.NONE, skill));
                        break;
                    case 2:
                        await session.EmitEventAsync(new DeleteTattooEvent(skill));
                        break;
                }
            }
                break;

            case UpgradePacketType.TATTOO_SAFEGUARD_SCROLL:
            {
                IBattleEntitySkill skill = session.PlayerEntity.Skills.FirstOrDefault(s => s.Skill.Id == upgradePacket.Data2);
                
                if (skill == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (upgradePacket.Data == 3)
                {
                    await session.EmitEventAsync(new UpgradeTattooEvent(TattooUpgradeProtection.TATTOO_SAFEGUARD_SCROLL, skill));
                }
            }
                break;

            case UpgradePacketType.CARVED_RUNE:
                
                if (upgradePacket.Data3 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                InventoryItem equipment = session.PlayerEntity.GetItemBySlotAndType((byte)upgradePacket.Data3, (InventoryType)upgradePacket.Data2);

                if (equipment == null)
                {
                    return;
                }

                switch (upgradePacket.Data)
                {
                    case 1:
                        if (equipment.ItemInstance.GameItem.EquipmentSlot is EquipmentType.MainWeapon)
                        {
                            await session.EmitEventAsync(new UpgradeWeaponCarvedRuneEvent(CarvedRunesUpgradeProtection.NONE, equipment));
                            return;
                        }
                        await session.EmitEventAsync(new UpgradeArmorCarvedRuneEvent(CarvedRunesUpgradeProtection.NONE, equipment));
                        break;
                    case 2:
                        await session.EmitEventAsync(new RemoveCarvedRuneEvent(equipment));
                        break;
                }
                break;

            case UpgradePacketType.PREMIUM_RUNE_OF_FORTUNE_SCROLL:

                if (upgradePacket.Data3 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (upgradePacket.Data4 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }
                
                equipment = session.PlayerEntity.GetItemBySlotAndType((byte)upgradePacket.Data3, (InventoryType)upgradePacket.Data2);

                if (equipment == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (upgradePacket.Data == 3)
                {
                    if (equipment.ItemInstance.GameItem.ItemType is ItemType.Weapon)
                    {
                        await session.EmitEventAsync(new UpgradeWeaponCarvedRuneEvent(CarvedRunesUpgradeProtection.PREMIUM_RUNE_OF_FORTUNE_SCROLL, equipment));
                        return;
                    }
                    await session.EmitEventAsync(new UpgradeArmorCarvedRuneEvent(CarvedRunesUpgradeProtection.PREMIUM_RUNE_OF_FORTUNE_SCROLL, equipment));
                }
                break;

            case UpgradePacketType.RUNE_OF_FORTUNE_SCROLL:

                if (upgradePacket.Data3 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (upgradePacket.Data4 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }
                
                equipment = session.PlayerEntity.GetItemBySlotAndType((byte)upgradePacket.Data3, (InventoryType)upgradePacket.Data2);

                if (equipment == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (upgradePacket.Data == 4)
                {
                    if (equipment.ItemInstance.GameItem.ItemType is ItemType.Weapon)
                    {
                        await session.EmitEventAsync(new UpgradeWeaponCarvedRuneEvent(CarvedRunesUpgradeProtection.RUNE_OF_FORTUNE_SCROLL, equipment));
                        return;
                    }
                    await session.EmitEventAsync(new UpgradeArmorCarvedRuneEvent(CarvedRunesUpgradeProtection.RUNE_OF_FORTUNE_SCROLL, equipment));
                }
                break;

            case UpgradePacketType.PREMIUM_RUNIC_UPGRADE_SCROLL:

                if (upgradePacket.Data3 == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                equipment = session.PlayerEntity.GetItemBySlotAndType((byte)upgradePacket.Data3, (InventoryType)upgradePacket.Data2);

                if (equipment == null)
                {
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    return;
                }

                if (upgradePacket.Data == 5)
                {
                    if (equipment.ItemInstance.GameItem.ItemType is ItemType.Weapon)
                    {
                        await session.EmitEventAsync(new UpgradeWeaponCarvedRuneEvent(CarvedRunesUpgradeProtection.PREMIUM_RUNIC_UPGRADE_SCROLL, equipment));
                        return;
                    }
                    await session.EmitEventAsync(new UpgradeArmorCarvedRuneEvent(CarvedRunesUpgradeProtection.PREMIUM_RUNIC_UPGRADE_SCROLL, equipment));
                }
                break;
        }
    }
}