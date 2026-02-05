using System;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game.BCard;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Monster;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Battle;

public class PlayerBattleEntityDump : IBattleEntityDump
{
    public PlayerBattleEntityDump(IPlayerEntity playerEntity, SkillInfo skill, bool isDefender, bool isMainTarget)
    {
        bool useSecondWeapon = false;
        GameItemInstance weapon = null;
        var bCardDictionary = new Dictionary<(BCardType type, byte subType), List<BCardDTO>>();

        if (skill != null) // get all info from attacker calculation
        {
            if (isDefender)
            {
                if (skill.BCardsType.TryGetValue(SkillCastType.BEFORE_ATTACK_ALL_TARGETS, out HashSet<BCardDTO> hashSetBCards))
                {
                    foreach (BCardDTO bCard in hashSetBCards)
                    {
                        if (!bCardDictionary.TryGetValue(((BCardType)bCard.Type, bCard.SubType), out List<BCardDTO> list))
                        {
                            list = new List<BCardDTO>();
                            bCardDictionary[((BCardType)bCard.Type, bCard.SubType)] = list;
                        }

                        list.Add(bCard);
                    }
                }
            }
            else
            {
                if (skill.BCardsType.TryGetValue(SkillCastType.BEFORE_ATTACK_SELF, out HashSet<BCardDTO> hashSetBCards))
                {
                    foreach (BCardDTO bCard in hashSetBCards)
                    {
                        if (!bCardDictionary.TryGetValue(((BCardType)bCard.Type, bCard.SubType), out List<BCardDTO> list))
                        {
                            list = new List<BCardDTO>();
                            bCardDictionary[((BCardType)bCard.Type, bCard.SubType)] = list;
                        }

                        list.Add(bCard);
                    }
                }

                HashSet<BCardDTO> chargeBCardToRemove = new();
                foreach (BCardDTO bCard in playerEntity.BCardComponent.GetChargeBCards())
                {
                    if (!bCardDictionary.TryGetValue(((BCardType)bCard.Type, bCard.SubType), out List<BCardDTO> list))
                    {
                        list = new List<BCardDTO>();
                        bCardDictionary[((BCardType)bCard.Type, bCard.SubType)] = list;
                    }

                    list.Add(bCard);
                    chargeBCardToRemove.Add(bCard);
                }

                foreach (BCardDTO bCard in chargeBCardToRemove)
                {
                    playerEntity.BCardComponent.RemoveChargeBCard(bCard);
                }
            }

            if (isMainTarget)
            {
                if (skill.BCardsType.TryGetValue(SkillCastType.BEFORE_ATTACK_ON_MAIN_TARGET, out HashSet<BCardDTO> hashSetBCards))
                {
                    foreach (BCardDTO bCard in hashSetBCards)
                    {
                        if (!bCardDictionary.TryGetValue(((BCardType)bCard.Type, bCard.SubType), out List<BCardDTO> list))
                        {
                            list = new List<BCardDTO>();
                            bCardDictionary[((BCardType)bCard.Type, bCard.SubType)] = list;
                        }

                        list.Add(bCard);
                    }
                }
            }

            switch (skill.AttackType)
            {
                case AttackType.Melee:
                    AttackType = AttackType.Melee;
                    switch (playerEntity.Class)
                    {
                        case ClassType.Archer:
                            DamageMinimum = playerEntity.SecondDamageMinimum;
                            DamageMaximum = playerEntity.SecondDamageMaximum;
                            HitRate = playerEntity.SecondHitRate;
                            CriticalChance = playerEntity.SecondHitCriticalChance;
                            CriticalDamage = playerEntity.SecondHitCriticalDamage;
                            useSecondWeapon = true;
                            {
                                GameItemInstance inventoryItem = playerEntity.SecondaryWeapon;
                                if (inventoryItem != null)
                                {
                                    weapon = inventoryItem;
                                }
                            }

                            break;
                        default:
                            {
                                GameItemInstance inventoryItem = playerEntity.MainWeapon;
                                if (inventoryItem != null)
                                {
                                    weapon = inventoryItem;
                                }
                            }
                            break;
                    }

                    break;

                case AttackType.Ranged:
                    AttackType = AttackType.Ranged;
                    switch (playerEntity.Class)
                    {
                        case ClassType.Adventurer:
                        case ClassType.Swordman:
                        case ClassType.Magician:
                        case ClassType.MartialArtist:
                            DamageMinimum = playerEntity.SecondDamageMinimum;
                            DamageMaximum = playerEntity.SecondDamageMaximum;
                            HitRate = playerEntity.SecondHitRate;
                            CriticalChance = playerEntity.SecondHitCriticalChance;
                            CriticalDamage = playerEntity.SecondHitCriticalDamage;
                            weapon = playerEntity.SecondaryWeapon;
                            useSecondWeapon = true;
                            break;
                        case ClassType.Archer:
                            weapon = playerEntity.MainWeapon;
                            break;
                    }

                    break;

                case AttackType.Magical:
                    AttackType = AttackType.Magical;
                    weapon = playerEntity.MainWeapon;
                    break;

                case AttackType.Other:
                    weapon = playerEntity.MainWeapon;
                    AttackType = playerEntity.Class switch
                    {
                        ClassType.MartialArtist => AttackType.Melee,
                        ClassType.Adventurer => AttackType.Melee,
                        ClassType.Swordman => AttackType.Melee,
                        ClassType.Archer => AttackType.Ranged,
                        ClassType.Magician => AttackType.Magical
                    };

                    break;

                case AttackType.Dash:
                    AttackType = AttackType.Melee;
                    weapon = playerEntity.Class switch
                    {
                        ClassType.Adventurer => playerEntity.MainWeapon,
                        ClassType.Swordman => playerEntity.MainWeapon,
                        ClassType.Archer => playerEntity.SecondaryWeapon,
                        ClassType.Magician => playerEntity.MainWeapon,
                        ClassType.MartialArtist => playerEntity.MainWeapon
                    };

                    if (playerEntity.Class == ClassType.Archer)
                    {
                        DamageMinimum = playerEntity.SecondDamageMinimum;
                        DamageMaximum = playerEntity.SecondDamageMaximum;
                        HitRate = playerEntity.SecondHitRate;
                        CriticalChance = playerEntity.SecondHitCriticalChance;
                        CriticalDamage = playerEntity.SecondHitCriticalDamage;
                        useSecondWeapon = true;
                    }

                    break;
            }
        }
        else
        {
            weapon = playerEntity.SecondaryWeapon;
            useSecondWeapon = true;
            AttackType = playerEntity.Class switch
            {
                ClassType.Adventurer => AttackType.Melee,
                ClassType.Swordman => AttackType.Melee,
                ClassType.MartialArtist => AttackType.Melee,
                ClassType.Archer => AttackType.Ranged,
                ClassType.Magician => AttackType.Magical
            };
        }

        if (weapon != null)
        {
            AttackUpgrade = weapon.Upgrade;
            WeaponDamageMinimum = weapon.DamageMinimum + weapon.GameItem.DamageMinimum;
            WeaponDamageMaximum = weapon.DamageMaximum + weapon.GameItem.DamageMaximum;
        }

        GameItemInstance armor = playerEntity.Armor;
        if (armor != null)
        {
            DefenseUpgrade = armor.Upgrade;
        }

        IReadOnlyList<BCardDTO> bCards = playerEntity.BCardComponent.GetAllBCards();
        foreach (BCardDTO bCard in bCards)
        {
            if (!bCardDictionary.TryGetValue(((BCardType)bCard.Type, bCard.SubType), out List<BCardDTO> list))
            {
                list = new List<BCardDTO>();
                bCardDictionary[((BCardType)bCard.Type, bCard.SubType)] = list;
            }

            list.Add(bCard);
        }

        ISet<int> buffsById = playerEntity.BuffComponent.GetAllBuffsId().ToHashSet();
        IReadOnlyList<(int casterLevel, BCardDTO bCard)> buffBCards = playerEntity.BCardComponent.GetBuffBCards();
        var buffs = new Dictionary<(BCardType type, byte subType), List<(int casterLevel, BCardDTO bCard)>>();
        foreach ((int casterLevel, BCardDTO bCard) in buffBCards)
        {
            (BCardType, byte SubType) key = ((BCardType)bCard.Type, bCard.SubType);
            if (!buffs.TryGetValue(key, out List<(int casterLevel, BCardDTO bCard)> list))
            {
                list = new List<(int casterLevel, BCardDTO bCard)>();
                buffs[key] = list;
            }

            list.Add((casterLevel, bCard));
        }

        short element = playerEntity.Element;

        BuffBCards = buffs;
        Hp = playerEntity.Hp;
        Mp = playerEntity.Mp;
        MaxHp = playerEntity.MaxHp;
        MaxMp = playerEntity.MaxMp;
        BuffsById = buffsById;
        BadBuffCounter = playerEntity.BuffComponent.GetAllBuffs(s => s.BuffGroup == BuffGroup.Bad).Count;
        Type = VisualType.Player;
        Id = playerEntity.Id;
        Morale = playerEntity.Level;
        Level = playerEntity.Level;
        Faction = playerEntity.Faction;
        DamageMinimum = !useSecondWeapon ? playerEntity.DamagesMinimum - WeaponDamageMinimum : DamageMinimum - WeaponDamageMinimum;
        DamageMaximum = !useSecondWeapon ? playerEntity.DamagesMaximum - WeaponDamageMaximum : DamageMaximum - WeaponDamageMaximum;
        CriticalChance = !useSecondWeapon ? playerEntity.HitCriticalChance : CriticalChance;
        CriticalDamage = !useSecondWeapon ? playerEntity.HitCriticalDamage : CriticalDamage;
        ShadowResistance = playerEntity.DarkResistance;
        LightResistance = playerEntity.LightResistance;
        WaterResistance = playerEntity.WaterResistance;
        FireResistance = playerEntity.FireResistance;
        HitRate = !useSecondWeapon ? playerEntity.HitRate : HitRate;
        MeleeDodge = playerEntity.MeleeDodge;
        RangeDodge = playerEntity.RangedDodge;
        Element = (ElementType)element;
        ElementRate = playerEntity.ElementRate + playerEntity.SpecialistElementRate;
        BCards = bCardDictionary;
        ShellOptionsWeapon = playerEntity.GetShellsValues(EquipmentOptionType.WEAPON_SHELL, !useSecondWeapon);
        ShellOptionArmor = playerEntity.GetShellsValues(EquipmentOptionType.ARMOR_SHELL, false);
        RingCellonValues = playerEntity.GetCellonValues(EquipmentType.Ring);
        NecklaceCellonValues = playerEntity.GetCellonValues(EquipmentType.Necklace);
        BraceletCellonValues = playerEntity.GetCellonValues(EquipmentType.Bracelet);
        Morale = playerEntity.Level;
        Position = new Position(playerEntity.PositionX, playerEntity.PositionY);
        MapInstance = playerEntity.MapInstance;
        MonsterRaceType = MonsterRaceType.People;
        MonsterRaceSubType = MonsterSubRace.People.Humanlike;
        MeleeDefense = playerEntity.MeleeDefence;
        RangeDefense = playerEntity.RangedDefence;
        MagicalDefense = playerEntity.MagicDefence;
        FamilyUpgrades = playerEntity.Family?.UpgradeValues ?? new Dictionary<FamilyUpgradeType, (short, short?)>();
        BCardDataComponent = new BCardDataComponent();

        double decreaseMagicDamage = 1;
        double damageReduction = 1;
        if (playerEntity.Specialist != null && playerEntity.UseSp)
        {
            IncreaseJajamaruDamage = playerEntity.Specialist.ItemVNum == (short)ItemVnums.JAJAMARU_SP &&
                playerEntity.MateComponent.GetTeamMember(x => x.MateType == MateType.Partner && x.MonsterVNum == (short)MonsterVnum.SAKURA) != null;

            int elementSl = playerEntity.SpecialistComponent.GetSlElement(playerEntity.Specialist.CurrentActiveSpecialistSlot);
            decreaseMagicDamage = elementSl switch
            {
                >= 20 and <= 49 => 0.95,
                >= 50 and <= 79 => 0.9,
                >= 80 and <= 99 => 0.85,
                >= 100 => 0.8,
                _ => decreaseMagicDamage
            };

            int slDefense = playerEntity.SpecialistComponent.GetSlDefense(playerEntity.Specialist.CurrentActiveSpecialistSlot);
            damageReduction = slDefense switch
            {
                >= 10 and < 40 => 0.85,
                >= 40 and < 80 => 0.5,
                >= 80 and < 100 => 0.25,
                100 => 0,
                _ => 1
            };
        }

        DecreaseMagicDamage = decreaseMagicDamage;
        MinimalDamageReduction = damageReduction;
    }

    public Dictionary<ShellEffectType, int> ShellOptionArmor { get; }
    public Dictionary<ShellEffectType, int> ShellOptionsWeapon { get; }
    public IReadOnlyDictionary<CellonType, int> RingCellonValues { get; }
    public IReadOnlyDictionary<CellonType, int> NecklaceCellonValues { get; }
    public IReadOnlyDictionary<CellonType, int> BraceletCellonValues { get; }
    public double MinimalDamageReduction { get; }
    public bool IncreaseJajamaruDamage { get; }
    public double DecreaseMagicDamage { get; }

    public IReadOnlyDictionary<(BCardType type, byte subType), List<BCardDTO>> BCards { get; }
    public IReadOnlyDictionary<(BCardType type, byte subType), List<(int casterLevel, BCardDTO bCard)>> BuffBCards { get; }
    public ISet<int> BuffsById { get; }
    public IReadOnlyDictionary<FamilyUpgradeType, (short, short?)> FamilyUpgrades { get; }
    public int BadBuffCounter { get; }
    public VisualType Type { get; }
    public long Id { get; }
    public IMapInstance MapInstance { get; }
    public MonsterRaceType MonsterRaceType { get; }
    public Enum MonsterRaceSubType { get; }
    public AttackType AttackType { get; }
    public ElementType Element { get; }
    public int ElementRate { get; }
    public int Morale { get; }
    public int Level { get; }
    public FactionType Faction { get; }
    public int DamageMinimum { get; }
    public int DamageMaximum { get; }
    public int AttackUpgrade { get; }
    public int DefenseUpgrade { get; }
    public int HitRate { get; }
    public int Hp { get; }
    public int Mp { get; }
    public int MaxHp { get; }
    public int MaxMp { get; }
    public int WeaponDamageMinimum { get; }
    public int WeaponDamageMaximum { get; }
    public int CriticalChance { get; }
    public int CriticalDamage { get; }
    
    public int CriticalDefence { get; }
    public int FireResistance { get; }
    public int WaterResistance { get; }
    public int LightResistance { get; }
    public int ShadowResistance { get; }
    public int MeleeDefense { get; }
    public int MeleeDodge { get; }
    public int RangeDefense { get; }
    public int RangeDodge { get; }
    public int MagicalDefense { get; }
    public Position Position { get; }
    public IBCardDataComponent BCardDataComponent { get; }
}