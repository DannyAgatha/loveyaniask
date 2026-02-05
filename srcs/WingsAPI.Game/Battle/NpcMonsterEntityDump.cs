using System;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Data.Families;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.BCard;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Maps;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.Game.Battle;

public class NpcMonsterEntityDump : IBattleEntityDump
{
    public NpcMonsterEntityDump(IMonsterData npcMonster, IBattleEntity battleEntity, SkillInfo skill, bool isDefender, bool isMainTarget)
    {
        Id = battleEntity.Id;
        Type = battleEntity.Type;
        Morale = battleEntity.Level;
        Level = npcMonster.BaseLevel;
        HeroLevel = npcMonster.BaseHeroLevel;
        Hp = battleEntity.Hp;
        Mp = battleEntity.Mp;
        MaxHp = battleEntity.MaxHp;
        MaxMp = battleEntity.MaxMp;
        var bCards = new Dictionary<(BCardType type, byte subType), List<BCardDTO>>();
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

        foreach (BCardDTO bCard in battleEntity.BCardComponent.GetAllBCards())
        {
            if (!bCards.TryGetValue(((BCardType)bCard.Type, bCard.SubType), out List<BCardDTO> list))
            {
                list = new List<BCardDTO>();
                bCards[((BCardType)bCard.Type, bCard.SubType)] = list;
            }

            list.Add(bCard);
        }

        foreach (BCardDTO bCard in battleEntity.BCardComponent.GetTriggerBCards(BCardTriggerType.ATTACK))
        {
            if (!bCards.TryGetValue(((BCardType)bCard.Type, bCard.SubType), out List<BCardDTO> list))
            {
                list = new List<BCardDTO>();
                bCards[((BCardType)bCard.Type, bCard.SubType)] = list;
            }

            list.Add(bCard);
        }

        foreach (BCardDTO bCard in battleEntity.BCardComponent.GetTriggerBCards(BCardTriggerType.DEFENSE))
        {
            if (!bCards.TryGetValue(((BCardType)bCard.Type, bCard.SubType), out List<BCardDTO> list))
            {
                list = new List<BCardDTO>();
                bCards[((BCardType)bCard.Type, bCard.SubType)] = list;
            }

            list.Add(bCard);
        }

        ISet<int> buffsById = battleEntity.BuffComponent.GetAllBuffsId().ToHashSet();
        IReadOnlyList<(int casterLevel, BCardDTO bCard)> buffBCards = battleEntity.BCardComponent.GetBuffBCards();
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

        BuffsById = buffsById;
        BadBuffCounter = battleEntity.BuffComponent.GetAllBuffs(s => s.BuffGroup == BuffGroup.Bad).Count;
        BCards = bCards;
        BuffBCards = buffs;
        DamageMinimum = npcMonster.BaseDamageMinimum;
        DamageMaximum = npcMonster.BaseDamageMaximum;
        WeaponDamageMinimum = 0;
        WeaponDamageMaximum = 0;
        HitRate = npcMonster.BaseConcentrate;
        CriticalChance = npcMonster.BaseCriticalChance;
        CriticalDamage = npcMonster.BaseCriticalRate;
        AttackType = npcMonster.AttackType;
        AttackUpgrade = npcMonster.AttackUpgrade;
        FireResistance = npcMonster.BaseFireResistance;
        WaterResistance = npcMonster.BaseWaterResistance;
        LightResistance = npcMonster.BaseLightResistance;
        ShadowResistance = npcMonster.BaseDarkResistance;
        AttackType = npcMonster.AttackType;
        DefenseUpgrade = npcMonster.DefenceUpgrade;
        MeleeDefense = npcMonster.BaseCloseDefence;
        MeleeDodge = npcMonster.DefenceDodge;
        RangeDefense = npcMonster.DistanceDefence;
        RangeDodge = npcMonster.DistanceDefenceDodge;
        MagicalDefense = npcMonster.MagicDefence;
        Element = (ElementType)npcMonster.BaseElement;
        ElementRate = npcMonster.BaseElementRate;
        Position = new Position(battleEntity.PositionX, battleEntity.PositionY);
        MapInstance = battleEntity.MapInstance;
        Faction = battleEntity.Faction;
        IsVesselMonster = battleEntity is IMonsterEntity { VesselMonster: true };
        IsVesselChristmasMonster = battleEntity is IMonsterEntity { VesselChristmasMonster: true };
        MonsterRaceType = npcMonster.MonsterRaceType;
        MonsterRaceSubType = npcMonster.GetMonsterRaceSubType();
        FamilyAttackAndDefense = 0;
        FamilyUpgrades = new Dictionary<FamilyUpgradeType, (short, short?)>();
        BCardDataComponent = new BCardDataComponent();

        if (battleEntity is not IMonsterEntity monsterEntity)
        {
            return;
        }

        if (skill == null)
        {
            return;
        }

        if (!monsterEntity.SummonerId.HasValue)
        {
            return;
        }

        if (!monsterEntity.SummonerType.HasValue && monsterEntity.SummonerType is not VisualType.Player)
        {
            return;
        }

        if (monsterEntity.MonsterVNum == (int)MonsterVnum.MINI_JAJAMARU)
        {
            return;
        }

        IBattleEntity summoner = monsterEntity.MapInstance.GetBattleEntity(monsterEntity.SummonerType.Value, monsterEntity.SummonerId.Value);

        if (summoner is not IPlayerEntity character)
        {
            return;
        }

        GameItemInstance weapon = character.MainWeapon;
        if (weapon != null)
        {
            AttackUpgrade = weapon.Upgrade;
            WeaponDamageMinimum = weapon.DamageMinimum + weapon.GameItem.DamageMinimum;
            WeaponDamageMaximum = weapon.DamageMaximum + weapon.GameItem.DamageMaximum;
        }

        DamageMinimum = character.DamagesMinimum + WeaponDamageMinimum;
        DamageMaximum = character.DamagesMaximum + WeaponDamageMaximum;
        HitRate = character.HitRate;
        Morale = character.Level;
        CriticalChance = character.HitCriticalChance;
        CriticalDamage = character.HitCriticalDamage;
    }

    public int FamilyAttackAndDefense { get; }
    public bool IsVesselMonster { get; }
    public bool IsVesselChristmasMonster { get; }

    public VisualType Type { get; }
    public long Id { get; }
    public IMapInstance MapInstance { get; }
    public MonsterRaceType MonsterRaceType { get; }
    public Enum MonsterRaceSubType { get; }
    public AttackType AttackType { get; }
    public ElementType Element { get; }
    public int ElementRate { get; }
    public int Morale { get; }
    public IReadOnlyDictionary<(BCardType type, byte subType), List<BCardDTO>> BCards { get; }
    public IReadOnlyDictionary<(BCardType type, byte subType), List<(int casterLevel, BCardDTO bCard)>> BuffBCards { get; }
    public ISet<int> BuffsById { get; }
    public IReadOnlyDictionary<FamilyUpgradeType, (short, short?)> FamilyUpgrades { get; }
    public int BadBuffCounter { get; }
    public int Level { get; }
    public byte HeroLevel { get; }
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