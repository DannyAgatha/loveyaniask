using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.BCard;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.Game.Battle;

public class MateBattleEntityDump : IBattleEntityDump
{
    public MateBattleEntityDump(IMateEntity mateEntity, SkillInfo skill, bool isDefender, bool isMainTarget)
    {
        GameItemInstance mateWeapon = mateEntity.Weapon;
        var bCards = new ConcurrentDictionary<(BCardType type, byte subType), List<BCardDTO>>();
        if (skill != null)
        {
            if (isDefender)
            {
                if (skill.BCardsType.TryGetValue(SkillCastType.BEFORE_ATTACK_ALL_TARGETS, out HashSet<BCardDTO> hashSetBCards))
                {
                    foreach (BCardDTO bCard in hashSetBCards)
                    {
                        if (!bCards.TryGetValue(((BCardType)bCard.Type, bCard.SubType), out List<BCardDTO> list))
                        {
                            list = new List<BCardDTO>();
                            bCards[((BCardType)bCard.Type, bCard.SubType)] = list;
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
                        if (!bCards.TryGetValue(((BCardType)bCard.Type, bCard.SubType), out List<BCardDTO> list))
                        {
                            list = new List<BCardDTO>();
                            bCards[((BCardType)bCard.Type, bCard.SubType)] = list;
                        }

                        list.Add(bCard);
                    }
                }
            }

            if (isMainTarget)
            {
                if (skill.BCardsType.TryGetValue(SkillCastType.BEFORE_ATTACK_ON_MAIN_TARGET, out HashSet<BCardDTO> hashSetBCards))
                {
                    foreach (BCardDTO bCard in hashSetBCards)
                    {
                        if (!bCards.TryGetValue(((BCardType)bCard.Type, bCard.SubType), out List<BCardDTO> list))
                        {
                            list = new List<BCardDTO>();
                            bCards[((BCardType)bCard.Type, bCard.SubType)] = list;
                        }

                        list.Add(bCard);
                    }
                }
            }
        }

        foreach (BCardDTO bCard in mateEntity.BCardComponent.GetAllBCards())
        {
            if (!bCards.TryGetValue(((BCardType)bCard.Type, bCard.SubType), out List<BCardDTO> list))
            {
                list = new List<BCardDTO>();
                bCards[((BCardType)bCard.Type, bCard.SubType)] = list;
            }

            list.Add(bCard);
        }

        foreach (BCardDTO bCard in mateEntity.BCardComponent.GetTriggerBCards(BCardTriggerType.ATTACK))
        {
            if (!bCards.TryGetValue(((BCardType)bCard.Type, bCard.SubType), out List<BCardDTO> list))
            {
                list = new List<BCardDTO>();
                bCards[((BCardType)bCard.Type, bCard.SubType)] = list;
            }

            list.Add(bCard);
        }

        foreach (BCardDTO bCard in mateEntity.BCardComponent.GetTriggerBCards(BCardTriggerType.DEFENSE))
        {
            if (!bCards.TryGetValue(((BCardType)bCard.Type, bCard.SubType), out List<BCardDTO> list))
            {
                list = new List<BCardDTO>();
                bCards[((BCardType)bCard.Type, bCard.SubType)] = list;
            }

            list.Add(bCard);
        }

        ISet<int> buffsById = mateEntity.BuffComponent.GetAllBuffsId().ToHashSet();
        IReadOnlyList<(int casterLevel, BCardDTO bCard)> buffBCards = mateEntity.BCardComponent.GetBuffBCards();
        var buffs = new ConcurrentDictionary<(BCardType type, byte subType), List<(int casterLevel, BCardDTO bCard)>>();
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

        BuffsById = buffsById;
        BadBuffCounter = mateEntity.BuffComponent.GetAllBuffs(s => s.BuffGroup == BuffGroup.Bad).Count;
        BCards = bCards;
        BuffBCards = buffs;
        Morale = mateEntity.Level;
        Id = mateEntity.Id;
        MapInstance = mateEntity.MapInstance;
        Level = mateEntity.Level;
        Type = VisualType.Npc;
        Faction = mateEntity.Faction;

        Position = new Position(mateEntity.PositionX, mateEntity.PositionY);
        MapInstance = mateEntity.MapInstance;

        Hp = mateEntity.Hp;
        Mp = mateEntity.Mp;
        MaxHp = mateEntity.MaxHp;
        MaxMp = mateEntity.MaxMp;

        HitRate = mateEntity.HitRate;
        CriticalChance = mateEntity.HitCriticalChance;
        CriticalDamage = mateEntity.HitCriticalDamage;
        CriticalDefence = mateEntity.MateType == MateType.Partner && mateEntity.IsUsingSp ? mateEntity.Specialist.SpCriticalDefense : 0;
        AttackUpgrade = mateEntity.Attack;
        FireResistance = mateEntity.FireResistance;
        WaterResistance = mateEntity.WaterResistance;
        LightResistance = mateEntity.LightResistance;
        ShadowResistance = mateEntity.DarkResistance;
        AttackType = mateEntity.AttackType;

        DefenseUpgrade = mateEntity.Defence;
        MeleeDefense = mateEntity.CloseDefence;
        RangeDefense = mateEntity.DistanceDefence;
        MagicalDefense = mateEntity.MagicDefence;
        MeleeDodge = mateEntity.DefenceDodge;
        RangeDodge = mateEntity.DistanceDodge;

        WeaponDamageMinimum = mateWeapon?.DamageMinimum + mateWeapon?.GameItem.DamageMinimum ?? 0;
        WeaponDamageMaximum = mateWeapon?.DamageMaximum + mateWeapon?.GameItem.DamageMaximum ?? 0;
        DamageMinimum = mateEntity.DamagesMinimum - WeaponDamageMinimum;
        DamageMaximum = mateEntity.DamagesMaximum - WeaponDamageMaximum;

        Element = (ElementType)mateEntity.Element;
        ElementRate = mateEntity.ElementRate;

        MonsterRaceType = mateEntity.MonsterRaceType;
        MonsterRaceSubType = mateEntity.GetMonsterRaceSubType();
        FamilyUpgrades = mateEntity.Owner.Family?.UpgradeValues ?? new Dictionary<FamilyUpgradeType, (short, short?)>();
        BCardDataComponent = new BCardDataComponent();
    }

    public VisualType Type { get; }
    public long Id { get; }
    public IMapInstance MapInstance { get; }
    public MonsterRaceType MonsterRaceType { get; }
    public Enum MonsterRaceSubType { get; }
    public AttackType AttackType { get; }
    public ElementType Element { get; }
    public int ElementRate { get; }
    public int Morale { get; }

    public IReadOnlyDictionary<(BCardType type, byte subType), List<(int casterLevel, BCardDTO bCard)>> BuffBCards { get; }
    public IReadOnlyDictionary<(BCardType type, byte subType), List<BCardDTO>> BCards { get; }
    public ISet<int> BuffsById { get; }
    public IReadOnlyDictionary<FamilyUpgradeType, (short, short?)> FamilyUpgrades { get; }

    public int BadBuffCounter { get; }
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