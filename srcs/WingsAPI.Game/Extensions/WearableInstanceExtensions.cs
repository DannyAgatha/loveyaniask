using System;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Extensions;

public static class ItemInstanceExtensions
{
    public static string GenerateInventoryAdd(this InventoryItem inventoryItem)
    {
        GameItemInstance item = inventoryItem.ItemInstance;
        int shellEffectUpgradeCount = (item.EquipmentOptions?.Count(shellEffect => shellEffect.ShellEffectUpgrade) ?? 0) is var resultat && resultat == 0 ? 0 : resultat + 80;

        switch (inventoryItem.InventoryType)
        {
            case InventoryType.Equipment:

                switch (item.GameItem.EquipmentSlot)
                {
                    case EquipmentType.Sp:
                        return $"ivn 0 {inventoryItem.Slot}.{item.ItemVNum}.{item.Rarity}.{item.Upgrade}.{item.SpStoneUpgrade}.0";
                    case EquipmentType.Armor:
                    case EquipmentType.MainWeapon:
                    case EquipmentType.SecondaryWeapon:
                        int upgrade = 0;

                        if (item.CarvedRunes != null)
                        {
                            upgrade = item.CarvedRunes.Upgrade;
                        }
                        
                        return $"ivn 0 {inventoryItem.Slot}.{item.ItemVNum}.{item.Rarity}.{(item.GameItem.IsColorable ? item.Design : item.Upgrade)}.{(upgrade > 0 ? "1" : "0")}.{upgrade}.{shellEffectUpgradeCount}";
                    default:
                        if (item.GameItem.ItemSubType == 7)
                        {
                            return $"ivn 0 {inventoryItem.Slot}.{item.ItemVNum}.{item.Rarity}.{(item.GameItem.IsColorable ? item.Design : item.Upgrade)}.{(item.IsBound ? 1 : 0)}.0";
                        }

                        return $"ivn 0 {inventoryItem.Slot}.{item.ItemVNum}.{item.Rarity}.{(item.GameItem.IsColorable ? item.Design : item.Upgrade)}.0.0";
                }

            case InventoryType.Main:
                return $"ivn 1 {inventoryItem.Slot}.{item.ItemVNum}.{item.Amount}.0";

            case InventoryType.Etc:
                return $"ivn 2 {inventoryItem.Slot}.{item.ItemVNum}.{item.Amount}.0";

            case InventoryType.Miniland:
                return $"ivn 3 {inventoryItem.Slot}.{item.ItemVNum}.{item.Amount}";

            case InventoryType.Specialist:
                return $"ivn 6 {inventoryItem.Slot}.{item.ItemVNum}.{item.Rarity}.{item.Upgrade}.{item.SpStoneUpgrade}";

            case InventoryType.Costume:
                return $"ivn 7 {inventoryItem.Slot}.{item.ItemVNum}.{item.Rarity}.{item.Upgrade}.0";
        }

        return string.Empty;
    }

    public static string GenerateSortItems(this IClientSession session, InventoryType inventoryType)
    {
        string specialist = "ivn 6";
        string costume = "ivn 7";
        string specialistList = string.Empty;
        string costumeList = string.Empty;
        IOrderedEnumerable<InventoryItem> items = session.PlayerEntity.GetItemsByInventoryType(inventoryType).OrderBy(x => x?.ItemInstance.ItemVNum);
        switch (inventoryType)
        {
            case InventoryType.Specialist:
                foreach (InventoryItem item in items)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    specialistList += $" {item.Slot}.{item.ItemInstance.ItemVNum}.{item.ItemInstance.Rarity}.{item.ItemInstance.Upgrade}.{item.ItemInstance.SpStoneUpgrade}";
                }

                return specialist + specialistList;
            case InventoryType.Costume:
                foreach (InventoryItem item in items)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    costumeList += $" {item.Slot}.{item.ItemInstance.ItemVNum}.{item.ItemInstance.Rarity}.{item.ItemInstance.Upgrade}.0";
                }

                return costume + costumeList;
        }

        return string.Empty;
    }

    public static void SendSortedItems(this IClientSession session, InventoryType inventoryType) => session.SendPacket(session.GenerateSortItems(inventoryType));

    public static int RarityPoint(this GameItemInstance item, short rarity, short lvl)
    {
        int p = rarity switch
        {
            0 => 0,
            1 => 1,
            2 => 2,
            3 => 3,
            4 => 4,
            5 => 5,
            6 => 7,
            7 => 10,
            8 => 15,
            _ => rarity * 2
        };

        return p * (lvl / 5 + 1);
    }

    public static string GenerateInventoryRemove(this InventoryItem item) => $"ivn {(byte)item.InventoryType} {item.Slot}.0.0.0";
    
    public static void SendSpecialistCardInfo(this IClientSession session, GameItemInstance specialistInstance, ICharacterAlgorithm characterAlgorithm)
        => session.SendPacket(session.GenerateSlInfo(specialistInstance, characterAlgorithm));

    public static string GenerateSlInfo(this IClientSession session, GameItemInstance specialistInstance, ICharacterAlgorithm algorithm)
    {
        int freePoint = specialistInstance.SpPointsBasic() - specialistInstance.SlDamage - specialistInstance.SlHp - specialistInstance.SlElement -
            specialistInstance.SlDefence;
        
        int slHit = specialistInstance.SlPoint(specialistInstance.SlDamage, SpecialistPointsType.ATTACK);
        int slDefence = specialistInstance.SlPoint(specialistInstance.SlDefence, SpecialistPointsType.DEFENCE);
        int slElement = specialistInstance.SlPoint(specialistInstance.SlElement, SpecialistPointsType.ELEMENT);
        int slHp = specialistInstance.SlPoint(specialistInstance.SlHp, SpecialistPointsType.HPMP);

        int shellHit = session.PlayerEntity.GetMaxWeaponShellValue(ShellEffectType.SLDamage) +
            session.PlayerEntity.GetMaxWeaponShellValue(ShellEffectType.SLGlobal) +
            session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseSpPoints,
                (byte)AdditionalTypes.IncreaseSpPoints.SpCardAttackPointIncrease, session.PlayerEntity.Level).firstData +
            session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.SPCardUpgrade,
                (byte)AdditionalTypes.SPCardUpgrade.IncreaseAllSkillPoints, session.PlayerEntity.Level).firstData;

        int shellDefense = session.PlayerEntity.GetMaxWeaponShellValue(ShellEffectType.SLDefence) +
            session.PlayerEntity.GetMaxWeaponShellValue(ShellEffectType.SLGlobal) +
            session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseSpPoints,
                (byte)AdditionalTypes.IncreaseSpPoints.SpCardDefensePointIncrease, session.PlayerEntity.Level).firstData +
            session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.SPCardUpgrade,
                (byte)AdditionalTypes.SPCardUpgrade.IncreaseAllSkillPoints, session.PlayerEntity.Level).firstData;

        int shellElement = session.PlayerEntity.GetMaxWeaponShellValue(ShellEffectType.SLElement) +
            session.PlayerEntity.GetMaxWeaponShellValue(ShellEffectType.SLGlobal) +
            session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseSpPoints,
                (byte)AdditionalTypes.IncreaseSpPoints.SpCardElementPointIncrease, session.PlayerEntity.Level).firstData +
            session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.SPCardUpgrade,
                (byte)AdditionalTypes.SPCardUpgrade.IncreaseAllSkillPoints, session.PlayerEntity.Level).firstData;

        int shellHp = session.PlayerEntity.GetMaxWeaponShellValue(ShellEffectType.SLHP) +
            session.PlayerEntity.GetMaxWeaponShellValue(ShellEffectType.SLGlobal) +
            session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseSpPoints,
                (byte)AdditionalTypes.IncreaseSpPoints.SpCardHpMpPointIncrease, session.PlayerEntity.Level).firstData +
            session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.SPCardUpgrade,
                (byte)AdditionalTypes.SPCardUpgrade.IncreaseAllSkillPoints, session.PlayerEntity.Level).firstData;
        
        bool spDestroyed = specialistInstance.Rarity == -2;

        string skill = string.Join(".", StaticSkillsManager.Instance.GetSkills()
            .Where(ski =>
                ski.UpgradeType == specialistInstance.GameItem.Morph
                && ski.SkillType == SkillType.NormalPlayerSkill
                && ski.LevelMinimum <= specialistInstance.SpLevel)
            .OrderBy(ski => ski.Id)
            .Select(ski => ski.Id));


        bool isStuff = specialistInstance.GameItem.Type is InventoryType.Specialist or InventoryType.Equipment or InventoryType.EquippedItems;

        return $"slinfo {(isStuff ? 0 : 2)} " +
            $"{specialistInstance.ItemVNum} " +
            $"{specialistInstance.GameItem.Morph} " +
            $"{specialistInstance.SpLevel} " +
            $"{specialistInstance.GameItem.LevelJobMinimum} " +
            $"{specialistInstance.GameItem.ReputationMinimum} " +
            $"{specialistInstance.SpAttackPassive} " +
            $"{specialistInstance.GameItem.Speed} " +
            $"{specialistInstance.SpCritProbabilityPassive} " +
            $"{specialistInstance.SpPercentBonusPassive} " +
            $"{specialistInstance.SpMeleeDefenceBonusPassive} " +
            $"{specialistInstance.SpRangedDefenceBonusPassive} " +
            $"{specialistInstance.SpMagicalDefenceBonusPassive} " +
            $"{specialistInstance.GameItem.SpPointsUsage} " +
            $"{specialistInstance.GameItem.FireResistance} " +
            $"{specialistInstance.GameItem.WaterResistance} " +
            $"{specialistInstance.GameItem.LightResistance} " +
            $"{specialistInstance.GameItem.DarkResistance} " +
            $"{specialistInstance.Xp} " +
            $"{algorithm.GetSpecialistJobXp(specialistInstance.SpLevel)} " +
            $"{skill} " +
            $"{specialistInstance.TransportId} " +
            $"{freePoint} " +
            $"{slHit} " +
            $"{slDefence} " +
            $"{slElement} " +
            $"{slHp} " +
            $"{specialistInstance.Upgrade} " +
            "-1 " + //TODO: Find it
            "0 " + //TODO: Find it
            $"{(spDestroyed ? 1 : 0)} " +
            $"{shellHit} " +
            $"{shellDefense} " +
            $"{shellElement} " +
            $"{shellHp} " +
            $"{specialistInstance.SpStoneUpgrade} " +
            $"{specialistInstance.SpDamage} " +
            $"{specialistInstance.SpDefence} " +
            $"{specialistInstance.SpElement} " +
            $"{specialistInstance.SpHP} " +
            $"{specialistInstance.SpFire} " +
            $"{specialistInstance.SpWater} " +
            $"{specialistInstance.SpLight} " +
            $"{specialistInstance.SpDark} " +
            $"{specialistInstance.SpGemElement} " +
            $"{specialistInstance.EquippedWingsVNum} " +
            $"{specialistInstance.AppearanceWingsVNum} " +
            $"{(specialistInstance.CanResetWingsAppearance ? 1 : 0)} " +
            "0 ";
    }

    public static string GetCarvedRunesInformation(this ItemInstanceDTO itemInstance, bool onlyUpgrade = false)
    {
        int bcardCount = 0;
        int upgrade = 0;
        bool isDamaged = false;

        if (onlyUpgrade)
        {
            return $"{itemInstance.CarvedRunes?.Upgrade ?? 0}";
        }
        
        if (itemInstance.CarvedRunes != null)
        {
            bcardCount = itemInstance.CarvedRunes.BCards?.Count ?? 0;
            isDamaged = itemInstance.CarvedRunes.IsDamaged;
            upgrade = itemInstance.CarvedRunes.Upgrade;
        }
        
        return $"{upgrade} {(isDamaged ? "1" : "0")} {bcardCount}";
    }
    
    public static int GetShellCount(this GameItemInstance itemInstance)
    {
        List<EquipmentOptionDTO> options = itemInstance.EquipmentOptions;

        if (options == null)
        {
            return 0;
        }

        if (!options.Any())
        {
            return 0;
        }

        EquipmentOptionDTO option = options.FirstOrDefault();

        if (option == null)
        {
            return 0;
        }

        if (option.EquipmentOptionType == EquipmentOptionType.RUNE)
        {
            return 0;
        }

        EquipmentType slot = option.EquipmentOptionType switch
        {
            EquipmentOptionType.ARMOR_SHELL => EquipmentType.Armor,
            EquipmentOptionType.WEAPON_SHELL => EquipmentType.MainWeapon,
            EquipmentOptionType.JEWELS => EquipmentType.Ring
        };

        int count = itemInstance.EquipmentOptions.Count(s => s.EquipmentOptionType == slot switch
        {
            EquipmentType.Armor => EquipmentOptionType.ARMOR_SHELL,
            EquipmentType.MainWeapon => EquipmentOptionType.WEAPON_SHELL,
            EquipmentType.SecondaryWeapon => EquipmentOptionType.WEAPON_SHELL,
            EquipmentType.Bracelet => EquipmentOptionType.JEWELS,
            EquipmentType.Necklace => EquipmentOptionType.JEWELS,
            EquipmentType.Ring => EquipmentOptionType.JEWELS
        });

        return count;
    }

    private static string GetRuneString(this GameItemInstance itemInstance)
    {
        if (itemInstance.CarvedRunes.BCards == null)
        {
            return string.Empty;
        }

        var sortedBCards = itemInstance.CarvedRunes.BCards.OrderBy(bCard => bCard.IsRunePower).ToList();
        
        string runeString = string.Empty;

        foreach (BCardDTO bCard in sortedBCards)
        {
            runeString += $" {bCard.Type}.{bCard.SubType / 10 - 1}.{((bCard.SubType % 10) == 1 ? bCard.FirstData * 4 : bCard.FirstData * 4 * -1)}.{bCard.SecondData * 4}.{(byte)bCard.FirstDataScalingType}";
        }
    
        return runeString;
    }

    private static string GetShellString(this GameItemInstance itemInstance)
    {
        if (itemInstance.EquipmentOptions == null)
        {
            return string.Empty;
        }

        var options = itemInstance.EquipmentOptions.Where(s => s.Level <= (s.Level > 12 ? 20 : 8))
            .OrderBy(s => s.Level).ToList();
        options.AddRange(itemInstance.EquipmentOptions.Where(s => s.Level > (s.Level > 12 ? 20 : 8))
            .OrderByDescending(s => s.Level));

        if (itemInstance.GameItem.ItemType == ItemType.Shell)
        {
            return options
                .Where(s => s.EquipmentOptionType == EquipmentOptionType.ARMOR_SHELL || s.EquipmentOptionType == EquipmentOptionType.WEAPON_SHELL)
                .Aggregate(string.Empty,
                    (current, option) => current + $" {(option.Level > 12 ? option.Level - 12 : option.Level)}.{(option.Type > 50 ? option.Type - 50 : option.Type)}.{option.Value}.");
        }

        switch (itemInstance.GameItem.EquipmentSlot)
        {
            case EquipmentType.Armor:
                return options
                    .Where(s => s.EquipmentOptionType == EquipmentOptionType.ARMOR_SHELL)
                    .OrderBy(s => s.Level)
                    .Aggregate(string.Empty,
                        (current, option) => current + $" {(option.Level > 12 ? option.Level - 12 : option.Level)}.{(option.Type > 50 ? option.Type - 50 : option.Type)}.{option.Value}.{(option.ShellEffectUpgrade ? -1 : 0)}");
            case EquipmentType.MainWeapon:
            case EquipmentType.SecondaryWeapon:
                return options
                    .Where(s => s.EquipmentOptionType == EquipmentOptionType.WEAPON_SHELL)
                    .OrderBy(s => s.Level)
                    .Aggregate(string.Empty, (current, option) => current + $" {option.Level}.{option.Type}.{option.Value}.{(option.ShellEffectUpgrade ? -1 : 0)}");
            case EquipmentType.Necklace:
            case EquipmentType.Bracelet:
            case EquipmentType.Ring:
            {
                return options
                    .Where(s => s.EquipmentOptionType == EquipmentOptionType.JEWELS)
                    .OrderBy(s => s.Level)
                    .Aggregate(string.Empty, (current, option) => current + $" {option.Type} {option.Level} {option.Value}");
            }
        }

        return "";
    }

    public static void SendEInfoPacket(this IClientSession session, GameItemInstance item, IItemsManager itemsManager, ICharacterAlgorithm characterAlgorithm) =>
        session.SendPacket(item.GenerateEInfo(itemsManager, characterAlgorithm, item.CurrentActiveSpecialistSlot));

    public static string GenerateEInfo(this GameItemInstance itemInstance, IItemsManager itemManager, ICharacterAlgorithm algorithm, byte activeSlot)
    {
        EquipmentType equipmentSlot = itemInstance.GameItem.EquipmentSlot;
        ItemType itemType = itemInstance.GameItem.ItemType;
        byte itemClass = itemInstance.GameItem.Class;
        byte subtype = itemInstance.GameItem.ItemSubType;

        long seconds;
        long hours;
        if (itemInstance.IsBound)
        {
            if (itemInstance.ItemDeleteTime == null)
            {
                seconds = 0;
                hours = 0;
            }
            else
            {
                seconds = (long)(itemInstance.ItemDeleteTime.Value - DateTime.UtcNow).TotalSeconds;
                hours = (long)(itemInstance.ItemDeleteTime.Value - DateTime.UtcNow).TotalHours;
            }
        }
        else
        {
            seconds = itemInstance.GameItem.ItemValidTime;
            hours = itemInstance.GameItem.ItemValidTime;
        }

        int shellEffectUpgradeCount = (itemInstance.EquipmentOptions?.Count(shellEffect => shellEffect.ShellEffectUpgrade) ?? 0) is var resultat && resultat == 0 ? 0 : resultat + 80;

        switch (itemType)
        {
            case ItemType.Weapon:
                switch (equipmentSlot)
                {
                    case EquipmentType.MiniPet:
                    case EquipmentType.MainWeapon:
                        {
                            byte eInfoType = 0;
                            switch (itemClass)
                            {
                                case 4:
                                    eInfoType = 1;
                                    break;
                                case 8:
                                    eInfoType = 5;
                                    break;
                            }

                            if (itemInstance.OriginalItemVnum != null)
                            {
                                return
                                    $"e_info {eInfoType} {itemInstance.ItemVNum} {itemInstance.Rarity} {itemInstance.Upgrade} {(itemInstance.IsFixed ? 1 : 0)} {itemInstance.GameItem.LevelMinimum} {itemInstance.GameItem.DamageMinimum + itemInstance.DamageMinimum} {itemInstance.GameItem.DamageMaximum + itemInstance.DamageMaximum} {itemInstance.GameItem.HitRate + itemInstance.HitRate} {itemInstance.GameItem.CriticalLuckRate} {itemInstance.GameItem.CriticalRate} {itemInstance.GameItem.Price} 0 0 {itemInstance.OriginalItemVnum} 0 0 0";
                            }

                            string shellStr = GetShellString(itemInstance);
                            string runesStr = GetRuneString(itemInstance);
                            return
                                $"e_info {eInfoType} {itemInstance.ItemVNum} {itemInstance.Rarity} {itemInstance.Upgrade} {(itemInstance.IsFixed ? 1 : 0)} {itemInstance.GameItem.LevelMinimum} {itemInstance.GameItem.DamageMinimum + itemInstance.DamageMinimum} {itemInstance.GameItem.DamageMaximum + itemInstance.DamageMaximum} {itemInstance.GameItem.HitRate + itemInstance.HitRate} {itemInstance.GameItem.CriticalLuckRate} {itemInstance.GameItem.CriticalRate} {itemInstance.Ammo} {itemInstance.GameItem.MaximumAmmo} {itemInstance.GameItem.Price} -1 0 {(itemInstance.BoundCharacterId == null ? "0" : $"{itemInstance.BoundCharacterId}")} {GetShellCount(itemInstance)} {shellEffectUpgradeCount}{shellStr} {GetCarvedRunesInformation(itemInstance)}{runesStr}";
                        }

                    case EquipmentType.SecondaryWeapon:
                        {
                            byte eInfoType = 0;
                            switch (itemClass)
                            {
                                case 1:
                                case 2:
                                    eInfoType = 1;
                                    break;
                            }

                            if (itemInstance.OriginalItemVnum != null)
                            {
                                return
                                    $"e_info {eInfoType} {itemInstance.ItemVNum} {itemInstance.Rarity} {itemInstance.Upgrade} {(itemInstance.IsFixed ? 1 : 0)} {itemInstance.GameItem.LevelMinimum} {itemInstance.GameItem.DamageMinimum + itemInstance.DamageMinimum} {itemInstance.GameItem.DamageMaximum + itemInstance.DamageMaximum} {itemInstance.GameItem.HitRate + itemInstance.HitRate} {itemInstance.GameItem.CriticalLuckRate} {itemInstance.GameItem.CriticalRate} {itemInstance.GameItem.Price} 0 0 {itemInstance.OriginalItemVnum} 0 0 0";
                            }

                            string shellStr = GetShellString(itemInstance);
                            string runesStr = GetRuneString(itemInstance);
                            return
                                $"e_info {eInfoType} {itemInstance.ItemVNum} {itemInstance.Rarity} {itemInstance.Upgrade} {(itemInstance.IsFixed ? 1 : 0)} {itemInstance.GameItem.LevelMinimum} {itemInstance.GameItem.DamageMinimum + itemInstance.DamageMinimum} {itemInstance.GameItem.DamageMaximum + itemInstance.DamageMaximum} {itemInstance.GameItem.HitRate + itemInstance.HitRate} {itemInstance.GameItem.CriticalLuckRate} {itemInstance.GameItem.CriticalRate} {itemInstance.Ammo} {itemInstance.GameItem.MaximumAmmo} {itemInstance.GameItem.Price} -1 0 {(itemInstance.BoundCharacterId == null ? "0" : $"{itemInstance.BoundCharacterId}")} {GetShellCount(itemInstance)} {shellEffectUpgradeCount}{shellStr} {GetCarvedRunesInformation(itemInstance)}{runesStr}";
                        }
                }

                break;

            case ItemType.Armor:
                {
                    byte eInfoType = 2;

                    if (itemInstance.OriginalItemVnum != null)
                    {
                        return
                            $"e_info {eInfoType} {itemInstance.ItemVNum} {itemInstance.Rarity} {itemInstance.Upgrade} {(itemInstance.IsFixed ? 1 : 0)} {itemInstance.GameItem.LevelMinimum} {itemInstance.GameItem.CloseDefence + itemInstance.CloseDefence} {itemInstance.GameItem.DistanceDefence + itemInstance.DistanceDefence} {itemInstance.GameItem.MagicDefence + itemInstance.MagicDefence} {itemInstance.GameItem.DefenceDodge + itemInstance.DefenceDodge} {itemInstance.GameItem.Price} {itemInstance.OriginalItemVnum} 0 0 0";
                    }

                    string shellStr = GetShellString(itemInstance);
                    string runesStr = GetRuneString(itemInstance);
                    return
                        $"e_info {eInfoType} {itemInstance.ItemVNum} {itemInstance.Rarity} {itemInstance.Upgrade} {(itemInstance.IsFixed ? 1 : 0)} {itemInstance.GameItem.LevelMinimum} {itemInstance.GameItem.CloseDefence + itemInstance.CloseDefence} {itemInstance.GameItem.DistanceDefence + itemInstance.DistanceDefence} {itemInstance.GameItem.MagicDefence + itemInstance.MagicDefence} {itemInstance.GameItem.DefenceDodge + itemInstance.DefenceDodge} {itemInstance.GameItem.Price} -1 0 {(itemInstance.BoundCharacterId == null ? "0" : $"{itemInstance.BoundCharacterId}")} {GetShellCount(itemInstance)} {shellEffectUpgradeCount}{shellStr} {GetCarvedRunesInformation(itemInstance)}{runesStr}";
                }

            case ItemType.Fashion:
                switch (equipmentSlot)
                {
                    case EquipmentType.CostumeHat:
                        return
                            $"e_info 3 {itemInstance.ItemVNum} {itemInstance.GameItem.LevelMinimum} {itemInstance.GameItem.CloseDefence + itemInstance.CloseDefence} {itemInstance.GameItem.DistanceDefence + itemInstance.DistanceDefence} {itemInstance.GameItem.MagicDefence + itemInstance.MagicDefence} {itemInstance.GameItem.DefenceDodge + itemInstance.DefenceDodge} {itemInstance.GameItem.FireResistance + itemInstance.FireResistance} {itemInstance.GameItem.WaterResistance + itemInstance.WaterResistance} {itemInstance.GameItem.LightResistance + itemInstance.LightResistance} {itemInstance.GameItem.DarkResistance + itemInstance.DarkResistance} {itemInstance.GameItem.Price} -1 {(itemInstance.IsBound ? 0 : 1)} {(itemInstance.GameItem.ItemValidTime == -1 ? -1 : seconds / 3600)} {itemInstance.Skin}";

                    case EquipmentType.CostumeSuit:
                        return
                            $"e_info 2 {itemInstance.ItemVNum} {itemInstance.Rarity} {itemInstance.Upgrade} {(itemInstance.IsFixed ? 1 : 0)} {itemInstance.GameItem.LevelMinimum} {itemInstance.GameItem.CloseDefence + itemInstance.CloseDefence} {itemInstance.GameItem.DistanceDefence + itemInstance.DistanceDefence} {itemInstance.GameItem.MagicDefence + itemInstance.MagicDefence} {itemInstance.GameItem.DefenceDodge + itemInstance.DefenceDodge} {itemInstance.GameItem.Price} -1 {(itemInstance.IsBound ? 0 : 1)} {(itemInstance.GameItem.ItemValidTime == -1 ? -1 : seconds / 3600)} {itemInstance.Skin}"; // 1 = IsCosmetic -1 = no shells

                    default:
                        return
                            $"e_info 3 {itemInstance.ItemVNum} {itemInstance.GameItem.LevelMinimum} {itemInstance.GameItem.CloseDefence + itemInstance.CloseDefence} {itemInstance.GameItem.DistanceDefence + itemInstance.DistanceDefence} {itemInstance.GameItem.MagicDefence + itemInstance.MagicDefence} {itemInstance.GameItem.DefenceDodge + itemInstance.DefenceDodge} {itemInstance.GameItem.FireResistance + itemInstance.FireResistance} {itemInstance.GameItem.WaterResistance + itemInstance.WaterResistance} {itemInstance.GameItem.LightResistance + itemInstance.LightResistance} {itemInstance.GameItem.DarkResistance + itemInstance.DarkResistance} {itemInstance.GameItem.Price} {itemInstance.Upgrade} 0 -1"; // after Item.Price theres TimesConnected {(Item.ItemValidTime == 0 ? -1 : Item.ItemValidTime / (3600))}
                }

            case ItemType.Jewelry:
                switch (equipmentSlot)
                {
                    case EquipmentType.Amulet:
                        if (itemInstance.DurabilityPoint > 0)
                        {
                            return
                                $"e_info 4 {itemInstance.ItemVNum} {itemInstance.GameItem.LevelMinimum} {itemInstance.DurabilityPoint} {itemInstance.GameItem.ItemLeftType} 0 {itemInstance.GameItem.Price}";
                        }

                        return $"e_info 4 {itemInstance.ItemVNum} {itemInstance.GameItem.LevelMinimum} {seconds * 10} {itemInstance.GameItem.ItemLeftType} 0 {itemInstance.GameItem.Price}";

                    case EquipmentType.Fairy:
                        // 0 nothing
                        // 1 can trade
                        // 2 can't trade 
                        int canTrade;
                        int levelMax;
                        int monsterNeed;
                        levelMax = itemInstance.GameItem.MaxElementRate == itemInstance.ElementRate + itemInstance.GameItem.ElementRate ? 1 : 2;
                        monsterNeed = (int)(algorithm.GetFairyXp((short)(itemInstance.ElementRate + itemInstance.GameItem.ElementRate)) - itemInstance.Xp);
                        if (itemInstance.GameItem.MaxElementRate == 70 || itemInstance.GameItem.MaxElementRate == 80)
                        {
                            canTrade = itemInstance.IsBound ? 2 : 0;
                        }
                        else
                        {
                            canTrade = 0;
                        }

                        return
                            $"e_info 4 {itemInstance.ItemVNum} {itemInstance.GameItem.Element} {itemInstance.ElementRate + itemInstance.GameItem.ElementRate} 0 {itemInstance.Xp} {itemInstance.GameItem.Price} {canTrade} {levelMax} {monsterNeed}"; // last IsNosmall

                    case EquipmentType.Necklace:
                    case EquipmentType.Bracelet:
                    case EquipmentType.Ring:
                        return
                            $"e_info 4 {itemInstance.ItemVNum} {itemInstance.GameItem.LevelMinimum} {itemInstance.GameItem.MaxCellonLvl} {itemInstance.GameItem.MaxCellon} {GetShellCount(itemInstance)} {itemInstance.GameItem.Price}{GetShellString(itemInstance)}";
                    
                    case EquipmentType.MiniPet:
                        return $"e_info 4 {itemInstance.ItemVNum} {itemInstance.GameItem.LevelMinimum} {hours} {itemInstance.GameItem.ItemLeftType} 0 {itemInstance.GameItem.Price}";

                    default:
                        return
                            $"e_info 4 {itemInstance.ItemVNum} {itemInstance.GameItem.LevelMinimum} {itemInstance.GameItem.MaxCellonLvl} {itemInstance.GameItem.MaxCellon} {itemInstance.Cellon} {itemInstance.GameItem.Price}";
                }

            case ItemType.Specialist:
                return $"e_info 8 {itemInstance.ItemVNum}";

            case ItemType.Box:

                // 0 = NOSMATE pearl 1= npc pearl 2 = sp box 3 = raid box 4= VEHICLE pearl
                // 5=fairy pearl
                switch (subtype)
                {
                    case 0:
                        return itemInstance.HoldingVNum is null or 0
                            ? $"e_info 7 {itemInstance.ItemVNum} 0"
                            : $"e_info 7 {itemInstance.ItemVNum} 1 {itemInstance.HoldingVNum} {itemInstance.SpLevel} {itemInstance.Xp} {algorithm.GetLevelXp(itemInstance.SpLevel, true, MateType.Pet)} {itemInstance.SpDamage} {itemInstance.SpDefence} {itemInstance.Stars}";

                    case 1:
                        return itemInstance.HoldingVNum is null or 0
                            ? $"e_info 7 {itemInstance.ItemVNum} 0"
                            : $"e_info 7 {itemInstance.ItemVNum} 1 {itemInstance.HoldingVNum} {itemInstance.SpLevel} {itemInstance.Xp} {algorithm.GetLevelXp(itemInstance.SpLevel, true)} {itemInstance.SpDamage} {itemInstance.SpDefence}";
                    case 2:
                        if (itemInstance.HoldingVNum is null or 0)
                        {
                            return $"e_info 7 {itemInstance.ItemVNum} 0";
                        }

                        IGameItem spitem = itemManager.GetItem(itemInstance.HoldingVNum.Value);
                        return
                            $"e_info 7 {itemInstance.ItemVNum} 1 {itemInstance.HoldingVNum} {itemInstance.SpLevel} {itemInstance.Xp} {algorithm.GetSpecialistJobXp(itemInstance.SpLevel)} {itemInstance.Upgrade} {itemInstance.SlPoint(itemInstance.SlDamage, SpecialistPointsType.ATTACK)} {itemInstance.SlPoint(itemInstance.SlDefence, SpecialistPointsType.DEFENCE)} {itemInstance.SlPoint(itemInstance.SlElement, SpecialistPointsType.ELEMENT)} {itemInstance.SlPoint(itemInstance.SlHp, SpecialistPointsType.HPMP)} {itemInstance.SpPointsBasic() - itemInstance.SlDamage - itemInstance.SlHp - itemInstance.SlElement- itemInstance.SlDefence} {itemInstance.SpStoneUpgrade} {spitem.FireResistance} {spitem.WaterResistance} {spitem.LightResistance} {spitem.DarkResistance} {itemInstance.SpDamage} {itemInstance.SpDefence} {itemInstance.SpElement} {itemInstance.SpHP} {itemInstance.SpFire} {itemInstance.SpWater} {itemInstance.SpLight} {itemInstance.SpDark}";

                    case 4:
                        return itemInstance.HoldingVNum is null or 0
                            ? $"e_info 11 {itemInstance.ItemVNum} 0"
                            : $"e_info 11 {itemInstance.ItemVNum} 1 {itemInstance.HoldingVNum}";

                    case 5:
                        if (itemInstance.HoldingVNum is null or 0)
                        {
                            return $"e_info 12 {itemInstance.ItemVNum} 0";
                        }

                        IGameItem fairyitem = itemManager.GetItem(itemInstance.HoldingVNum.Value);
                        return itemInstance.HoldingVNum == 0
                            ? $"e_info 12 {itemInstance.ItemVNum} 0"
                            : $"e_info 12 {itemInstance.ItemVNum} 1 {itemInstance.HoldingVNum} {itemInstance.ElementRate + fairyitem.ElementRate}";

                    case 6:
                        if (itemInstance.HoldingVNum is null or 0)
                        {
                            return $"e_info 12 {itemInstance.ItemVNum} 0";
                        }

                        byte? itemElement = itemManager.GetItem(itemInstance.HoldingVNum.Value)?.Element;
                        return $"e_info 13 {itemInstance.ItemVNum} 1 {itemInstance.HoldingVNum} {itemElement} {itemInstance.GenerateSkillInfo(2).Replace('.', ' ')} {itemInstance.SpStoneUpgrade} {itemInstance.SpDamage} {itemInstance.SpDefence} {itemInstance.SpCriticalDefense} {itemInstance.SpHP} {itemInstance.SpFire} {itemInstance.SpWater} {itemInstance.SpLight} {itemInstance.SpDark}";

                    case 7:
                        return $"e_info 11 {itemInstance.ItemVNum} {(itemInstance.IsBound ? 1 : 0)} {hours}";

                    default:
                        return $"e_info 8 {itemInstance.ItemVNum} -1 {itemInstance.Rarity}";
                }

            case ItemType.Shell:
                return
                    $"e_info 9 {itemInstance.ItemVNum} {itemInstance.Upgrade} {itemInstance.Rarity} {itemInstance.GameItem.Price} {GetShellCount(itemInstance)}{GetShellString(itemInstance)}";
        }

        return string.Empty;
    }

    public static void SetRarityPoint(this GameItemInstance itemInstance, IRandomGenerator randomGenerator)
    {
        if (itemInstance.Type != ItemInstanceType.WearableInstance || itemInstance.Rarity == 0)
        {
            return;
        }

        int multiplier;
        switch (itemInstance.Rarity)
        {
            case -2:
                multiplier = -20;
                break;
            case -1:
                multiplier = -10;
                break;
            case 0:
                multiplier = 0;
                break;
            case 1:
                multiplier = 1;
                break;
            case 2:
                multiplier = 2;
                break;
            case 3:
                multiplier = 3;
                break;
            case 4:
                multiplier = 4;
                break;
            case 5:
                multiplier = 5;
                break;
            case 6:
                multiplier = 7;
                break;
            case 7:
            case 8:
                multiplier = 10;
                break;
            default:
                multiplier = 0;
                break;
        }

        if (multiplier == 0)
        {
            return;
        }

        double itemFix = itemInstance.GameItem.IsHeroic ? 0.05 : 0;
        short itemLevel = itemInstance.GameItem.LevelMinimum;

        if (itemInstance.GameItem.IsHeroic)
        {
            itemLevel = (short)Math.Floor(100.0 + (itemInstance.GameItem.LevelMinimum - 20.0) / 4.0);
        }

        switch (itemInstance.GameItem.EquipmentSlot)
        {
            case EquipmentType.MainWeapon:
            case EquipmentType.SecondaryWeapon:

                if (itemInstance.Rarity > 0)
                {
                    int additionalAttack = (int)Math.Floor((itemLevel / 5 + 1 + itemFix) * multiplier);
                    if (itemInstance.Rarity == 8)
                    {
                        additionalAttack += randomGenerator.RandomNumber(40, 101);
                    }

                    int minMultiplier = (int)Math.Floor(itemLevel / 25.0 * multiplier);
                    int maxMultiplier = (int)Math.Floor((itemLevel / 20.0 + 1) * multiplier) + (int)Math.Floor(itemLevel / 40.0 * multiplier);
                    int hitRateMultiplier = randomGenerator.RandomNumber(minMultiplier, maxMultiplier);


                    itemInstance.WeaponMinDamageAdditionalValue = additionalAttack - hitRateMultiplier;
                    itemInstance.WeaponMaxDamageAdditionalValue = additionalAttack - hitRateMultiplier;
                    itemInstance.WeaponHitRateAdditionalValue = hitRateMultiplier;
                }
                else
                {
                    int minMultiplier = multiplier;
                    int maxMultiplier = multiplier;

                    itemInstance.WeaponMinDamageAdditionalValue = minMultiplier;
                    itemInstance.WeaponMaxDamageAdditionalValue = maxMultiplier;
                }

                break;

            case EquipmentType.Armor:

                if (itemInstance.Rarity > 0)
                {
                    int additionalArmor = (int)Math.Floor((itemLevel / 5 + 1 + itemFix) * multiplier);
                    if (itemInstance.Rarity == 8)
                    {
                        additionalArmor += randomGenerator.RandomNumber(40, 101);
                    }

                    int minDifferent = (int)Math.Floor(itemLevel / 75.0 * multiplier);
                    int maxDifferent = (int)Math.Floor((itemLevel / 30.0 + 1.0) * multiplier);
                    int dodgeAdditionalValue = randomGenerator.RandomNumber(minDifferent, maxDifferent);

                    int minDifferentMagic = (int)Math.Floor(itemLevel / 50.0 * multiplier);
                    int maxDifferentMagic = (int)Math.Floor((itemLevel / 25.0 + 1.0) * multiplier);
                    int armorMagicAdditionalValue = randomGenerator.RandomNumber(minDifferentMagic, maxDifferentMagic);

                    int multiplierLeft = randomGenerator.RandomNumber(70, 131);
                    int left = additionalArmor - dodgeAdditionalValue - armorMagicAdditionalValue;
                    int armorRangeAdditionalValue = (int)Math.Floor(left / 2.0 * (multiplierLeft * 0.01));
                    int armorMeleeAdditionalValue = left - armorRangeAdditionalValue;

                    itemInstance.ArmorDodgeAdditionalValue = dodgeAdditionalValue;
                    itemInstance.ArmorMeleeAdditionalValue = armorMeleeAdditionalValue;
                    itemInstance.ArmorRangeAdditionalValue = armorRangeAdditionalValue;
                    itemInstance.ArmorMagicAdditionalValue = armorMagicAdditionalValue;
                }
                else
                {
                    itemInstance.ArmorMeleeAdditionalValue = multiplier;
                    itemInstance.ArmorRangeAdditionalValue = multiplier;
                    itemInstance.ArmorMagicAdditionalValue = multiplier;
                }

                break;
        }
    }
}