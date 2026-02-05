using System;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Bonus;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.EntityStatistics;

public class PlayerStatisticsComponent : IPlayerStatisticsComponent
{
    private readonly Dictionary<PassiveType, int> _passive = new();
    private readonly IPlayerEntity _playerEntity;
    private readonly Dictionary<Statistics, int> _stats = new();

    public PlayerStatisticsComponent(IPlayerEntity playerEntity) => _playerEntity = playerEntity;

    public IReadOnlyDictionary<PassiveType, int> Passives => _passive;

    public void RefreshPassives()
    {
        _passive.Clear();
        IEnumerable<SkillDTO> passiveSkills = _playerEntity.CharacterSkills.Values.Where(x => x?.Skill != null && x.Skill.IsPassiveSkill()).Select(x => x.Skill);

        int hp = 0;
        int mp = 0;
        int meleeAttack = 0;
        int rangedAttack = 0;
        int magicAttack = 0;
        int regenHp = 0;
        int regenMp = 0;
        int passiveRegen = 1;
        int meleeDefence = 0;
        int rangedDefence = 0;
        int magicDefence = 0;
        int meleeHitrate = 0;
        int rangedHitrate = 0;
        int meleeDodge = 0;
        int rangedDodge = 0;
        int pvpdamage = 0;
        int pvpdefence = 0;
        int resistance = 0;
        int experience = 0;
        int gold = 0;

        hp += _playerEntity.Family?.UpgradeValues.GetValueOrDefault(FamilyUpgradeType.MAX_HP).Item1 ?? 0;
        mp += _playerEntity.Family?.UpgradeValues.GetValueOrDefault(FamilyUpgradeType.INCREASE_MAX_MP).Item1 ?? 0;

        foreach (SkillDTO skill in passiveSkills)
        {
            switch (skill.CastId)
            {
                case 0:
                    meleeAttack += skill.UpgradeSkill;
                    meleeDefence += skill.UpgradeSkill;
                    break;
                case 1:
                    rangedAttack += skill.UpgradeSkill;
                    rangedDefence += skill.UpgradeSkill;
                    meleeHitrate += skill.UpgradeSkill;
                    rangedHitrate += skill.UpgradeSkill;
                    meleeDodge += skill.UpgradeSkill;
                    rangedDodge += skill.UpgradeSkill;
                    break;
                case 2:
                    magicAttack += skill.UpgradeSkill;
                    magicDefence += skill.UpgradeSkill;
                    break;
                case 4:
                    hp += skill.UpgradeSkill;
                    break;
                case 5:
                    mp += skill.UpgradeSkill;
                    break;
                case 6:
                    meleeAttack += skill.UpgradeSkill;
                    rangedAttack += skill.UpgradeSkill;
                    magicAttack += skill.UpgradeSkill;
                    break;
                case 7:
                    meleeDefence += skill.UpgradeSkill;
                    rangedDefence += skill.UpgradeSkill;
                    magicDefence += skill.UpgradeSkill;
                    break;
                case 8:
                    regenHp += skill.UpgradeSkill;
                    break;
                case 9:
                    regenMp += skill.UpgradeSkill;
                    break;
                case 10:
                    passiveRegen += skill.UpgradeSkill;
                    break;

                // HERO BOOK
                case 19:
                    pvpdamage += skill.UpgradeType;
                    break;
                case 20:
                    pvpdefence += skill.UpgradeType;
                    break;
                case 21:
                    meleeAttack += skill.UpgradeType;
                    meleeDefence += skill.UpgradeType;
                    break;
                case 22:
                    rangedAttack += skill.UpgradeType;
                    rangedDefence += skill.UpgradeType;
                    meleeHitrate += skill.UpgradeType;
                    rangedHitrate += skill.UpgradeType;
                    meleeDodge += skill.UpgradeType;
                    rangedDodge += skill.UpgradeType;
                    break;
                case 23:
                    magicAttack += skill.UpgradeType;
                    magicDefence += skill.UpgradeType;
                    break;
                case 24:
                    hp += skill.UpgradeType;
                    break;
                case 25:
                    mp += skill.UpgradeType;
                    break;
                case 26:
                    meleeDefence += skill.UpgradeType;
                    rangedDefence += skill.UpgradeType;
                    magicDefence += skill.UpgradeType;
                    break;
                case 27:
                    meleeAttack += skill.UpgradeType;
                    rangedAttack += skill.UpgradeType;
                    magicAttack += skill.UpgradeType;
                    break;
                case 28:
                    resistance += skill.UpgradeType;
                    break;
                case 29:
                    experience += skill.UpgradeType;
                    break;
                case 30:
                    gold += skill.UpgradeType;
                    break;
                case 31:
                    hp += skill.UpgradeType;
                    break;
                case 32:
                    rangedAttack += skill.UpgradeType;
                    rangedDefence += skill.UpgradeType;
                    meleeHitrate += skill.UpgradeType;
                    rangedHitrate += skill.UpgradeType;
                    meleeDodge += skill.UpgradeType;
                    rangedDodge += skill.UpgradeType;
                    break;
                case 33:
                    magicAttack += skill.UpgradeType;
                    magicDefence += skill.UpgradeType;
                    break;
                case 34:
                    meleeAttack += skill.UpgradeType;
                    meleeDefence += skill.UpgradeType;
                    break;
                case 35:
                    mp += skill.UpgradeType;
                    break;
                case 36:
                case 37:
                case 127:
                case 128:
                case 129:
                    meleeAttack += skill.UpgradeType;
                    rangedAttack += skill.UpgradeType;
                    magicAttack += skill.UpgradeType;
                    meleeDefence += skill.UpgradeType;
                    rangedDefence += skill.UpgradeType;
                    magicDefence += skill.UpgradeType;
                    break;
            }
        }

        switch (_playerEntity.SubClass)
        {
            case SubClassType.OathKeeper:
                hp += 1140 + (_playerEntity.TierLevel - 1) * 230;
                mp += 200 + (_playerEntity.TierLevel - 1) * 40;
                break;

            case SubClassType.CrimsonFury:
                hp += 1000 + (_playerEntity.TierLevel - 1) * 200;
                mp += 200 + (_playerEntity.TierLevel - 1) * 40;
                break;

            case SubClassType.CelestialPaladin:
                hp += 1420 + (_playerEntity.TierLevel - 1) * 280;
                mp += 200 + (_playerEntity.TierLevel - 1) * 40;
                break;

            case SubClassType.SilentStalker:
                hp += 800 + (_playerEntity.TierLevel - 1) * 150;
                mp += 190 + (_playerEntity.TierLevel - 1) * 40;
                break;

            case SubClassType.ArrowLord:
                hp += 950 + (_playerEntity.TierLevel - 1) * 140;
                mp += 250 + (_playerEntity.TierLevel - 1) * 50;
                break;

            case SubClassType.ShadowHunter:
                hp += 950 + (_playerEntity.TierLevel - 1) * 140;
                mp += 190 + (_playerEntity.TierLevel - 1) * 40;
                break;

            case SubClassType.ArcaneSage:
                hp += 500 + (_playerEntity.TierLevel - 1) * 100;
                mp += 380 + (_playerEntity.TierLevel - 1) * 70;
                break;

            case SubClassType.Pyromancer:
                hp += 580 + (_playerEntity.TierLevel - 1) * 110;
                mp += 380 + (_playerEntity.TierLevel - 1) * 70;
                break;

            case SubClassType.DarkNecromancer:
                hp += 500 + (_playerEntity.TierLevel - 1) * 100;
                mp += 440 + (_playerEntity.TierLevel - 1) * 70;
                break;

            case SubClassType.ZenWarrior:
                hp += 1060 + (_playerEntity.TierLevel - 1) * 210;
                mp += 190 + (_playerEntity.TierLevel - 1) * 40;
                break;

            case SubClassType.EmperorsBlade:
                hp += 920 + (_playerEntity.TierLevel - 1) * 180;
                mp += 190 + (_playerEntity.TierLevel - 1) * 40;
                break;

            case SubClassType.StealthShadow:
                hp += 800 + (_playerEntity.TierLevel - 1) * 150;
                mp += 190 + (_playerEntity.TierLevel - 1) * 40;
                break;
        }
        
        hp += _playerEntity.HaveStaticBonus(StaticBonusType.OrderOfDiscipline) ? 3000 : 0;
        mp += _playerEntity.HaveStaticBonus(StaticBonusType.OrderOfDiscipline) ? 3000 : 0;


        _passive[PassiveType.HP] = hp;
        _passive[PassiveType.MP] = mp;
        _passive[PassiveType.MELEE_ATTACK] = meleeAttack;
        _passive[PassiveType.RANGED_ATTACK] = rangedAttack;
        _passive[PassiveType.MAGIC_ATTACK] = magicAttack;
        _passive[PassiveType.REGEN_HP] = regenHp;
        _passive[PassiveType.REGEN_MP] = regenMp;
        _passive[PassiveType.PASSIVE_REGEN] = passiveRegen;
        _passive[PassiveType.MELEE_DEFENCE] = meleeDefence;
        _passive[PassiveType.RANGED_DEFENCE] = rangedDefence;
        _passive[PassiveType.MAGIC_DEFENCE] = magicDefence;
        _passive[PassiveType.MELEE_HIRATE] = meleeHitrate;
        _passive[PassiveType.RANGED_HIRATE] = rangedHitrate;
        _passive[PassiveType.MELEE_DODGE] = meleeDodge;
        _passive[PassiveType.RANGED_DODGE] = rangedDodge;

        // HERO BOOK
        _passive[PassiveType.PVP_DAMAGE_HERO_BOOK] = pvpdamage;
        _passive[PassiveType.PVP_DEFENCE_HERO_BOOK] = pvpdefence;
        _passive[PassiveType.ALL_ELEMENT_RESISTANCES_HERO_BOOK] = resistance;
        _passive[PassiveType.EXPERIENCE_HERO_BOOK] = experience;
        _passive[PassiveType.GOLD_HERO_BOOK] = gold;
        
    }
    
    public int MinDamage
    {
        get
        {
            int baseMinDamage = _playerEntity.DamagesMinimum;
            int addedMinDamage = _stats.GetValueOrDefault(Statistics.MIN_DAMAGE);
            int totalMinDamage = baseMinDamage + addedMinDamage;
            
            return totalMinDamage;
        }
    }
    
    public int MaxDamage
    {
        get
        {
            int baseMaxDamage = _playerEntity.DamagesMaximum;
            int addedMaxDamage = _stats.GetValueOrDefault(Statistics.MAX_DAMAGE);
            int totalMaxDamage = baseMaxDamage + addedMaxDamage;
            
            return totalMaxDamage;
        }
    }

    public int HitRate => _playerEntity.HitRate + _stats.GetValueOrDefault(Statistics.HITRATE);
    public int CriticalChance => _playerEntity.HitCriticalChance + _stats.GetValueOrDefault(Statistics.CRITICAL_CHANCE);
    public int CriticalDamage => _playerEntity.HitCriticalDamage + _stats.GetValueOrDefault(Statistics.CRITICAL_DAMAGE);
    public int SecondMinDamage => _playerEntity.SecondDamageMinimum + _stats.GetValueOrDefault(Statistics.SECOND_MIN_DAMAGE);
    public int SecondMaxDamage => _playerEntity.SecondDamageMaximum + _stats.GetValueOrDefault(Statistics.SECOND_MAX_DAMAGE);
    public int SecondHitRate => _playerEntity.SecondHitRate + _stats.GetValueOrDefault(Statistics.SECOND_HITRATE);
    public int SecondCriticalChance => _playerEntity.SecondHitCriticalChance + _stats.GetValueOrDefault(Statistics.SECOND_CRITICAL_CHANCE);
    public int SecondCriticalDamage => _playerEntity.SecondHitCriticalDamage + _stats.GetValueOrDefault(Statistics.SECOND_CRITICAL_DAMAGE);
    public int MeleeDefense => _playerEntity.MeleeDefence + _stats.GetValueOrDefault(Statistics.MELEE_DEFENSE);
    public int RangeDefense => _playerEntity.RangedDefence + _stats.GetValueOrDefault(Statistics.RANGE_DEFENSE);
    public int MagicDefense => _playerEntity.MagicDefence + _stats.GetValueOrDefault(Statistics.MAGIC_DEFENSE);
    public int MeleeDodge => _playerEntity.MeleeDodge + _stats.GetValueOrDefault(Statistics.MELEE_DODGE);
    public int RangeDodge => _playerEntity.RangedDodge + _stats.GetValueOrDefault(Statistics.RANGE_DODGE);
    public int FireResistance
    {
        get
        {
            int baseFireResistance = _playerEntity.FireResistance
                + _stats.GetValueOrDefault(Statistics.FIRE_RESISTANCE)
                + _playerEntity.StatisticsComponent.Passives.GetValueOrDefault(PassiveType.ALL_ELEMENT_RESISTANCES_HERO_BOOK);

            double totalIncreasePercentage = _playerEntity.BCardComponent.GetAllBCards()
                .Where(bCard => bCard.Type == (short)BCardType.IncreaseElementByResis
                    && bCard.SubType is (byte)AdditionalTypes.IncreaseElementByResis.FireElementResisIncrease
                        or (byte)AdditionalTypes.IncreaseElementByResis.AllElementResisIncrease)
                .Sum(bCard => bCard.FirstDataValue(_playerEntity.Level)) * 0.01;

            int increasedFireResistance = (int)Math.Round(baseFireResistance * (1 + totalIncreasePercentage));

            return increasedFireResistance;
        }
    }

    public int WaterResistance
    {
        get
        {
            int baseWaterResistance = _playerEntity.WaterResistance
                + _stats.GetValueOrDefault(Statistics.WATER_RESISTANCE)
                + _playerEntity.StatisticsComponent.Passives.GetValueOrDefault(PassiveType.ALL_ELEMENT_RESISTANCES_HERO_BOOK);

            double totalIncreasePercentage = _playerEntity.BCardComponent.GetAllBCards()
                .Where(bCard => bCard.Type == (short)BCardType.IncreaseElementByResis
                    && bCard.SubType is (byte)AdditionalTypes.IncreaseElementByResis.WaterElementResisIncrease
                        or (byte)AdditionalTypes.IncreaseElementByResis.AllElementResisIncrease)
                .Sum(bCard => bCard.FirstDataValue(_playerEntity.Level)) * 0.01;

            int increasedWaterResistance = (int)Math.Round(baseWaterResistance * (1 + totalIncreasePercentage));

            return increasedWaterResistance;
        }
    }
    public int LightResistance
    {
        get
        {
            int baseLightResistance = _playerEntity.LightResistance
                + _stats.GetValueOrDefault(Statistics.LIGHT_RESISTANCE)
                + _playerEntity.StatisticsComponent.Passives.GetValueOrDefault(PassiveType.ALL_ELEMENT_RESISTANCES_HERO_BOOK);

            double totalIncreasePercentage = _playerEntity.BCardComponent.GetAllBCards()
                .Where(bCard => bCard.Type == (short)BCardType.IncreaseElementByResis
                    && bCard.SubType is (byte)AdditionalTypes.IncreaseElementByResis.LightElementResisIncrease
                        or (byte)AdditionalTypes.IncreaseElementByResis.AllElementResisIncrease)
                .Sum(bCard => bCard.FirstDataValue(_playerEntity.Level)) * 0.01;

            int increasedLightResistance = (int)Math.Round(baseLightResistance * (1 + totalIncreasePercentage));

            return increasedLightResistance;
        }
    }
    public int ShadowResistance
    {
        get
        {
            int baseShadowResistance = _playerEntity.DarkResistance
                + _stats.GetValueOrDefault(Statistics.SHADOW_RESISTANCE)
                + _playerEntity.StatisticsComponent.Passives.GetValueOrDefault(PassiveType.ALL_ELEMENT_RESISTANCES_HERO_BOOK);

            double totalIncreasePercentage = _playerEntity.BCardComponent.GetAllBCards()
                .Where(bCard => bCard.Type == (short)BCardType.IncreaseElementByResis
                    && bCard.SubType is (byte)AdditionalTypes.IncreaseElementByResis.ShadowElementResisIncrease
                        or (byte)AdditionalTypes.IncreaseElementByResis.AllElementResisIncrease)
                .Sum(bCard => bCard.FirstDataValue(_playerEntity.Level)) * 0.01;

            int increasedShadowResistance = (int)Math.Round(baseShadowResistance * (1 + totalIncreasePercentage));

            return increasedShadowResistance;
        }
    }
    
    public void RefreshPlayerStatistics()
    {
        _stats.Clear();
        int minDamage = 0;
        int maxDamage = 0;
        int hitRate = 0;
        int criticalChance = 0;
        int criticalDamage = 0;
        int secondMinDamage = 0;
        int secondMaxDamage = 0;
        int secondHitRate = 0;
        int secondCriticalChance = 0;
        int secondCriticalDamage = 0;
        int meleeDefense = 0;
        int rangeDefense = 0;
        int magicDefense = 0;
        int meleeDodge = 0;
        int rangeDodge = 0;
        int fireResistance = 0;
        int waterResistance = 0;
        int lightResistance = 0;
        int shadowResistance = 0;

        ClassType classType = _playerEntity.Class;

        byte playerLevel = _playerEntity.Level;

        IReadOnlyList<BCardDTO> bCards = _playerEntity.BCardComponent.GetAllBCards();
        IEnumerable<BCardDTO> minMaxDamageBCards = bCards.Where(x => x.Type == (short)BCardType.AttackPower);
        IEnumerable<BCardDTO> hitRateBCards = bCards.Where(x => x.Type == (short)BCardType.Target);
        IEnumerable<BCardDTO> criticalBCards = bCards.Where(x => x.Type == (short)BCardType.Critical);
        IEnumerable<BCardDTO> defenseBCards = bCards.Where(x => x.Type == (short)BCardType.Defence);
        IEnumerable<BCardDTO> dodgeBCards = bCards.Where(x => x.Type == (short)BCardType.DodgeAndDefencePercent);
        IEnumerable<BCardDTO> resistanceBCards = bCards.Where(x => x.Type == (short)BCardType.ElementResistance);

        foreach (BCardDTO bCard in minMaxDamageBCards)
        {
            int firstData = bCard.FirstDataValue(playerLevel);

            switch ((AdditionalTypes.AttackPower)bCard.SubType)
            {
                case AdditionalTypes.AttackPower.AllAttacksIncreased:
                    minDamage += firstData;
                    maxDamage += firstData;
                    secondMinDamage += firstData;
                    secondMaxDamage += firstData;
                    break;
                case AdditionalTypes.AttackPower.AllAttacksDecreased:
                    minDamage -= firstData;
                    maxDamage -= firstData;
                    secondMinDamage -= firstData;
                    secondMaxDamage -= firstData;
                    break;
                case AdditionalTypes.AttackPower.MeleeAttacksIncreased:
                    minDamage += classType switch
                    {
                        ClassType.Adventurer => firstData,
                        ClassType.Swordman => firstData,
                        ClassType.MartialArtist => firstData,
                        _ => 0
                    };

                    maxDamage += classType switch
                    {
                        ClassType.Adventurer => firstData,
                        ClassType.Swordman => firstData,
                        ClassType.MartialArtist => firstData,
                        _ => 0
                    };

                    secondMinDamage += classType switch
                    {
                        ClassType.Archer => firstData,
                        _ => 0
                    };

                    secondMaxDamage += classType switch
                    {
                        ClassType.Archer => firstData,
                        _ => 0
                    };
                    break;
                case AdditionalTypes.AttackPower.MeleeAttacksDecreased:
                    minDamage -= classType switch
                    {
                        ClassType.Adventurer => firstData,
                        ClassType.Swordman => firstData,
                        ClassType.MartialArtist => firstData,
                        _ => 0
                    };

                    maxDamage -= classType switch
                    {
                        ClassType.Adventurer => firstData,
                        ClassType.Swordman => firstData,
                        ClassType.MartialArtist => firstData,
                        _ => 0
                    };

                    secondMinDamage -= classType switch
                    {
                        ClassType.Archer => firstData,
                        _ => 0
                    };

                    secondMaxDamage -= classType switch
                    {
                        ClassType.Archer => firstData,
                        _ => 0
                    };
                    break;
                case AdditionalTypes.AttackPower.RangedAttacksIncreased:
                    minDamage += classType switch
                    {
                        ClassType.Archer => firstData,
                        _ => 0
                    };

                    maxDamage += classType switch
                    {
                        ClassType.Archer => firstData,
                        _ => 0
                    };

                    if (classType == ClassType.Archer)
                    {
                        break;
                    }

                    secondMinDamage += firstData;
                    secondMaxDamage += firstData;
                    break;
                case AdditionalTypes.AttackPower.RangedAttacksDecreased:
                    minDamage -= classType switch
                    {
                        ClassType.Archer => firstData,
                        _ => 0
                    };

                    maxDamage -= classType switch
                    {
                        ClassType.Archer => firstData,
                        _ => 0
                    };

                    if (classType == ClassType.Archer)
                    {
                        break;
                    }

                    secondMinDamage -= firstData;
                    secondMaxDamage -= firstData;
                    break;
                case AdditionalTypes.AttackPower.MagicalAttacksIncreased:
                    if (classType != ClassType.Magician)
                    {
                        break;
                    }

                    minDamage += firstData;
                    maxDamage += firstData;
                    break;
                case AdditionalTypes.AttackPower.MagicalAttacksDecreased:
                    if (classType != ClassType.Magician)
                    {
                        break;
                    }

                    minDamage -= firstData;
                    maxDamage -= firstData;
                    break;
            }
        }

        foreach (BCardDTO bCard in hitRateBCards)
        {
            int firstData = bCard.FirstDataValue(playerLevel);
            switch ((AdditionalTypes.Target)bCard.SubType)
            {
                case AdditionalTypes.Target.AllHitRateIncreased:
                    hitRate += firstData;
                    secondHitRate += firstData;
                    break;
                case AdditionalTypes.Target.AllHitRateDecreased:
                    hitRate -= firstData;
                    secondHitRate -= firstData;
                    break;
                case AdditionalTypes.Target.MeleeHitRateIncreased:
                    hitRate += classType switch
                    {
                        ClassType.Adventurer => firstData,
                        ClassType.Swordman => firstData,
                        ClassType.MartialArtist => firstData,
                        _ => 0
                    };

                    secondHitRate += classType switch
                    {
                        ClassType.Archer => firstData,
                        _ => 0
                    };
                    break;
                case AdditionalTypes.Target.MeleeHitRateDecreased:
                    hitRate -= classType switch
                    {
                        ClassType.Adventurer => firstData,
                        ClassType.Swordman => firstData,
                        ClassType.MartialArtist => firstData,
                        _ => 0
                    };

                    secondHitRate -= classType switch
                    {
                        ClassType.Archer => firstData,
                        _ => 0
                    };
                    break;
                case AdditionalTypes.Target.RangedHitRateIncreased:
                    hitRate += classType switch
                    {
                        ClassType.Archer => firstData,
                        _ => 0
                    };

                    if (classType == ClassType.Archer)
                    {
                        break;
                    }

                    secondHitRate += firstData;
                    break;
                case AdditionalTypes.Target.RangedHitRateDecreased:
                    hitRate -= classType switch
                    {
                        ClassType.Archer => firstData,
                        _ => 0
                    };

                    if (classType == ClassType.Archer)
                    {
                        break;
                    }

                    secondHitRate -= firstData;
                    break;
                case AdditionalTypes.Target.MagicalConcentrationIncreased:
                    if (classType != ClassType.Magician)
                    {
                        break;
                    }

                    hitRate += firstData;
                    break;
                case AdditionalTypes.Target.MagicalConcentrationDecreased:
                    if (classType != ClassType.Magician)
                    {
                        break;
                    }

                    hitRate -= firstData;
                    break;
            }
            
            switch ((AdditionalTypes.IncreaseAllDamage)bCard.SubType)
            {
                case AdditionalTypes.IncreaseAllDamage.ConcentrationIncrease:
                    if (classType != ClassType.Magician)
                    {
                        break;
                    }
                    hitRate += firstData;
                    break;
                case AdditionalTypes.IncreaseAllDamage.ConcentrationDecrease:
                    if (classType != ClassType.Magician)
                    {
                        break;
                    }
                    hitRate -= firstData;
                    break;
            }
        }

        foreach (BCardDTO bCard in criticalBCards)
        {
            int firstData = bCard.FirstDataValue(playerLevel);

            switch ((AdditionalTypes.Critical)bCard.SubType)
            {
                case AdditionalTypes.Critical.InflictingIncreased:
                    criticalChance += classType switch
                    {
                        ClassType.Adventurer => firstData,
                        ClassType.Swordman => firstData,
                        ClassType.Archer => firstData,
                        ClassType.MartialArtist => firstData,
                        _ => 0
                    };

                    secondCriticalChance += firstData;
                    break;
                case AdditionalTypes.Critical.InflictingReduced:
                    criticalChance -= classType switch
                    {
                        ClassType.Adventurer => firstData,
                        ClassType.Swordman => firstData,
                        ClassType.Archer => firstData,
                        ClassType.MartialArtist => firstData,
                        _ => 0
                    };

                    secondCriticalChance -= firstData;
                    break;
                case AdditionalTypes.Critical.DamageIncreased:
                    criticalDamage += classType switch
                    {
                        ClassType.Adventurer => firstData,
                        ClassType.Swordman => firstData,
                        ClassType.Archer => firstData,
                        ClassType.MartialArtist => firstData,
                        _ => 0
                    };

                    secondCriticalDamage += firstData;
                    break;
                case AdditionalTypes.Critical.DamageIncreasedInflictingReduced:
                    criticalDamage -= classType switch
                    {
                        ClassType.Adventurer => firstData,
                        ClassType.Swordman => firstData,
                        ClassType.Archer => firstData,
                        ClassType.MartialArtist => firstData,
                        _ => 0
                    };

                    secondCriticalDamage -= firstData;
                    break;
            }
        }

        foreach (BCardDTO bCard in defenseBCards)
        {
            int firstData = bCard.FirstDataValue(playerLevel);

            switch ((AdditionalTypes.Defence)bCard.SubType)
            {
                case AdditionalTypes.Defence.AllIncreased:
                    meleeDefense += firstData;
                    rangeDefense += firstData;
                    magicDefense += firstData;
                    break;
                case AdditionalTypes.Defence.AllDecreased:
                    meleeDefense -= firstData;
                    rangeDefense -= firstData;
                    magicDefense -= firstData;
                    break;
                case AdditionalTypes.Defence.MeleeIncreased:
                    meleeDefense += firstData;
                    break;
                case AdditionalTypes.Defence.MeleeDecreased:
                    meleeDefense -= firstData;
                    break;
                case AdditionalTypes.Defence.RangedIncreased:
                    rangeDefense += firstData;
                    break;
                case AdditionalTypes.Defence.RangedDecreased:
                    rangeDefense -= firstData;
                    break;
                case AdditionalTypes.Defence.MagicalIncreased:
                    magicDefense += firstData;
                    break;
                case AdditionalTypes.Defence.MagicalDecreased:
                    magicDefense -= firstData;
                    break;
            }
        }

        foreach (BCardDTO bCard in dodgeBCards)
        {
            int firstData = bCard.FirstDataValue(playerLevel);

            switch (bCard.SubType)
            {
                case (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeIncreased:
                    meleeDodge += firstData;
                    rangeDodge += firstData;
                    break;
                case (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeDecreased:
                    meleeDodge -= firstData;
                    rangeDodge -= firstData;
                    break;
                case (byte)AdditionalTypes.DodgeAndDefencePercent.DodgingMeleeIncreased:
                    meleeDodge += firstData;
                    break;
                case (byte)AdditionalTypes.DodgeAndDefencePercent.DodgingMeleeDecreased:
                    meleeDodge -= firstData;
                    break;
                case (byte)AdditionalTypes.DodgeAndDefencePercent.DodgingRangedIncreased:
                    rangeDodge += firstData;
                    break;
                case (byte)AdditionalTypes.DodgeAndDefencePercent.DodgingRangedDecreased:
                    rangeDodge -= firstData;
                    break;
            }
        }

        foreach (BCardDTO bCard in resistanceBCards)
        {
            int firstData = bCard.FirstDataValue(playerLevel);

            switch (bCard.SubType)
            {
                case (byte)AdditionalTypes.ElementResistance.AllIncreased:
                    fireResistance += firstData;
                    waterResistance += firstData;
                    lightResistance += firstData;
                    shadowResistance += firstData;
                    break;
                case (byte)AdditionalTypes.ElementResistance.AllDecreased:
                    fireResistance -= firstData;
                    waterResistance -= firstData;
                    lightResistance -= firstData;
                    shadowResistance -= firstData;
                    break;
                case (byte)AdditionalTypes.ElementResistance.FireIncreased:
                    fireResistance += firstData;
                    break;
                case (byte)AdditionalTypes.ElementResistance.FireDecreased:
                    fireResistance -= firstData;
                    break;
                case (byte)AdditionalTypes.ElementResistance.WaterIncreased:
                    waterResistance += firstData;
                    break;
                case (byte)AdditionalTypes.ElementResistance.WaterDecreased:
                    waterResistance -= firstData;
                    break;
                case (byte)AdditionalTypes.ElementResistance.LightIncreased:
                    lightResistance += firstData;
                    break;
                case (byte)AdditionalTypes.ElementResistance.LightDecreased:
                    lightResistance -= firstData;
                    break;
                case (byte)AdditionalTypes.ElementResistance.DarkIncreased:
                    shadowResistance += firstData;
                    break;
                case (byte)AdditionalTypes.ElementResistance.DarkDecreased:
                    shadowResistance -= firstData;
                    break;
            }
        }
        

        minDamage += _playerEntity.GetMaxWeaponShellValue(ShellEffectType.DamageImproved, true);
        maxDamage += _playerEntity.GetMaxWeaponShellValue(ShellEffectType.DamageImproved, true);
        secondMinDamage += _playerEntity.GetMaxWeaponShellValue(ShellEffectType.DamageImproved, false);
        secondMaxDamage += _playerEntity.GetMaxWeaponShellValue(ShellEffectType.DamageImproved, false);

        criticalChance += _playerEntity.GetMaxWeaponShellValue(ShellEffectType.CriticalChance, true);
        criticalDamage += _playerEntity.GetMaxWeaponShellValue(ShellEffectType.CriticalDamage, true);
        secondCriticalChance += _playerEntity.GetMaxWeaponShellValue(ShellEffectType.CriticalChance, false);
        secondCriticalDamage += _playerEntity.GetMaxWeaponShellValue(ShellEffectType.CriticalDamage, false);

        meleeDefense += _playerEntity.GetMaxArmorShellValue(ShellEffectType.CloseDefence);
        rangeDefense += _playerEntity.GetMaxArmorShellValue(ShellEffectType.DistanceDefence);
        magicDefense += _playerEntity.GetMaxArmorShellValue(ShellEffectType.MagicDefence);

        fireResistance += _playerEntity.GetMaxArmorShellValue(ShellEffectType.IncreasedFireResistance);
        waterResistance += _playerEntity.GetMaxArmorShellValue(ShellEffectType.IncreasedWaterResistance);
        lightResistance += _playerEntity.GetMaxArmorShellValue(ShellEffectType.IncreasedLightResistance);
        shadowResistance += _playerEntity.GetMaxArmorShellValue(ShellEffectType.IncreasedDarkResistance);

        fireResistance += _playerEntity.GetMaxArmorShellValue(ShellEffectType.IncreasedAllResistance);
        waterResistance += _playerEntity.GetMaxArmorShellValue(ShellEffectType.IncreasedAllResistance);
        lightResistance += _playerEntity.GetMaxArmorShellValue(ShellEffectType.IncreasedAllResistance);
        shadowResistance += _playerEntity.GetMaxArmorShellValue(ShellEffectType.IncreasedAllResistance);

        fireResistance += _playerEntity.Family?.UpgradeValues?.GetOrDefault(FamilyUpgradeType.FIRE_RESISTANCE).Item1 ?? 0;
        waterResistance += _playerEntity.Family?.UpgradeValues?.GetOrDefault(FamilyUpgradeType.WATER_RESISTANCE).Item1 ?? 0;
        lightResistance += _playerEntity.Family?.UpgradeValues?.GetOrDefault(FamilyUpgradeType.LIGHT_RESISTANCE).Item1 ?? 0;
        shadowResistance += _playerEntity.Family?.UpgradeValues?.GetOrDefault(FamilyUpgradeType.DARK_RESISTANCE).Item1 ?? 0;

        switch (_playerEntity.SubClass)
        {
            case SubClassType.OathKeeper:
                minDamage += 40 + (_playerEntity.TierLevel - 1) * 30;
                maxDamage += 40 + (_playerEntity.TierLevel - 1) * 30;
                hitRate += 40 + (_playerEntity.TierLevel - 1) * 40;
                criticalChance += 4 + (_playerEntity.TierLevel - 1);
                criticalDamage += 23 + (_playerEntity.TierLevel - 1); 
                meleeDefense += 20 + (_playerEntity.TierLevel - 1) * 30;
                rangeDefense += 20 + (_playerEntity.TierLevel - 1) * 10;
                meleeDodge += 20 + (_playerEntity.TierLevel - 1) * 30;
                fireResistance += 10 + (_playerEntity.TierLevel - 1); 
                waterResistance += 10 + (_playerEntity.TierLevel - 1); 
                lightResistance += 10 + (_playerEntity.TierLevel - 1); 
                shadowResistance += 10 + (_playerEntity.TierLevel - 1);
                break;

            case SubClassType.CrimsonFury:
                minDamage += 50 + (_playerEntity.TierLevel - 1) * 40;
                maxDamage += 50 + (_playerEntity.TierLevel - 1) * 40;
                hitRate += 40 + (_playerEntity.TierLevel - 1) * 30;
                criticalChance += 5 + (_playerEntity.TierLevel - 1); 
                criticalDamage += 26 + (_playerEntity.TierLevel - 1); 
                meleeDefense += 20 + (_playerEntity.TierLevel - 1) * 20;
                rangeDefense += 20 + (_playerEntity.TierLevel - 1) * 10;
                magicDefense += 10 + (_playerEntity.TierLevel - 1) * 10;
                meleeDodge += 30 + (_playerEntity.TierLevel - 1) * 30;
                fireResistance += 9 + (_playerEntity.TierLevel - 1); 
                waterResistance += 9 + (_playerEntity.TierLevel - 1); 
                lightResistance += 9 + (_playerEntity.TierLevel - 1); 
                shadowResistance += 9 + (_playerEntity.TierLevel - 1); 
                break;

            case SubClassType.CelestialPaladin:
                minDamage += 30 + (_playerEntity.TierLevel - 1) * 30;
                maxDamage += 30 + (_playerEntity.TierLevel - 1) * 30;
                hitRate += 40 + (_playerEntity.TierLevel - 1) * 30;
                criticalChance += 4 + (_playerEntity.TierLevel - 1); 
                criticalDamage += 22 + (_playerEntity.TierLevel - 1); 
                meleeDefense += 20 + (_playerEntity.TierLevel - 1) * 30;
                magicDefense += 20 + (_playerEntity.TierLevel - 1) * 10;
                meleeDodge += 20 + (_playerEntity.TierLevel - 1) * 30;
                fireResistance += 10 + (_playerEntity.TierLevel - 1); 
                waterResistance += 10 + (_playerEntity.TierLevel - 1); 
                lightResistance += 10 + (_playerEntity.TierLevel - 1); 
                shadowResistance += 10 + (_playerEntity.TierLevel - 1);
                break;

            case SubClassType.SilentStalker:
                minDamage += 30 + (_playerEntity.TierLevel - 1) * 40 - Math.Max(0, (_playerEntity.TierLevel - 3) * 10);
                maxDamage += 30 + (_playerEntity.TierLevel - 1) * 40 - Math.Max(0, (_playerEntity.TierLevel - 3) * 10);
                hitRate += 60 + (_playerEntity.TierLevel - 1) * 60;
                criticalChance += 6 + Math.Max(0, (_playerEntity.TierLevel - 3) * 2);
                criticalDamage += 30 + (_playerEntity.TierLevel - 1) + Math.Max(0, (_playerEntity.TierLevel - 4) * 1);
                meleeDefense += 10 + (_playerEntity.TierLevel - 1) * 10;
                rangeDefense += 10 + Math.Min(40, (_playerEntity.TierLevel - 1) * 20);
                magicDefense += 10 + Math.Min(40, (_playerEntity.TierLevel - 1) * 20) + Math.Max(0, (_playerEntity.TierLevel - 4) * 10);
                meleeDodge += 50 + (_playerEntity.TierLevel - 1) * 50 + Math.Max(0, (_playerEntity.TierLevel - 2) * 10);
                fireResistance += 5 + Math.Max(0, _playerEntity.TierLevel - 2);
                waterResistance += 5 + Math.Max(0, _playerEntity.TierLevel - 2); 
                lightResistance += 5 + Math.Max(0, _playerEntity.TierLevel - 2); 
                shadowResistance += 5 + Math.Max(0, _playerEntity.TierLevel - 2);
                break;

            case SubClassType.ArrowLord:
                minDamage += 40 + (_playerEntity.TierLevel - 1) * 40 - Math.Max(0, (_playerEntity.TierLevel - 2) * 10);
                maxDamage += 40 + (_playerEntity.TierLevel - 1) * 40 - Math.Max(0, (_playerEntity.TierLevel - 2) * 10);
                hitRate += 50 + (_playerEntity.TierLevel - 1) * 50 - Math.Max(0, (_playerEntity.TierLevel - 3) * 10);
                criticalChance += 6 + Math.Max(0, (_playerEntity.TierLevel - 3) * 2);
                criticalDamage += 30 + (_playerEntity.TierLevel - 1) + Math.Max(0, (_playerEntity.TierLevel - 4) * 1);
                meleeDefense += 10 + (_playerEntity.TierLevel - 1) * 10;
                rangeDefense += 10 + Math.Min(40, (_playerEntity.TierLevel - 1) * 20);
                magicDefense += 10 + Math.Min(40, (_playerEntity.TierLevel - 1) * 20) + Math.Max(0, (_playerEntity.TierLevel - 4) * 10);
                meleeDodge += 40 + (_playerEntity.TierLevel - 1) * 30 + Math.Max(0, (_playerEntity.TierLevel - 3) * 10);
                fireResistance += 5 + Math.Max(0, _playerEntity.TierLevel - 2);
                waterResistance += 5 + Math.Max(0, _playerEntity.TierLevel - 2);
                lightResistance += 5 + Math.Max(0, _playerEntity.TierLevel - 2);
                shadowResistance += 5 + Math.Max(0, _playerEntity.TierLevel - 2);
                break;

            case SubClassType.ShadowHunter:
                minDamage += 30 + (_playerEntity.TierLevel - 1) * 40 - Math.Max(0, (_playerEntity.TierLevel - 3) * 10);
                maxDamage += 30 + (_playerEntity.TierLevel - 1) * 40 - Math.Max(0, (_playerEntity.TierLevel - 3) * 10);
                hitRate += 40 + (_playerEntity.TierLevel - 1) * 30 + Math.Max(0, (_playerEntity.TierLevel - 3) * 10);
                criticalChance += 7 + Math.Max(0, (_playerEntity.TierLevel - 3) * 2);
                criticalDamage += 35 + (_playerEntity.TierLevel - 1);
                meleeDefense += 10 + (_playerEntity.TierLevel - 1) * 10;
                rangeDefense += 10 + Math.Min(40, (_playerEntity.TierLevel - 1) * 20);
                magicDefense += 10 + Math.Min(40, (_playerEntity.TierLevel - 1) * 20) + Math.Max(0, (_playerEntity.TierLevel - 4) * 10);
                meleeDodge += 40 + (_playerEntity.TierLevel - 1) * 40 + Math.Max(0, (_playerEntity.TierLevel - 2) * 10);
                fireResistance += 5 + Math.Max(0, _playerEntity.TierLevel - 2);
                waterResistance += 5 + Math.Max(0, _playerEntity.TierLevel - 2);
                lightResistance += 5 + Math.Max(0, _playerEntity.TierLevel - 2);
                shadowResistance += 5 + Math.Max(0, _playerEntity.TierLevel - 2);
                break;

            case SubClassType.ArcaneSage:
                minDamage += 40 + (_playerEntity.TierLevel - 1) * 30 + Math.Max(0, (_playerEntity.TierLevel - 4) * 10);
                maxDamage += 40 + (_playerEntity.TierLevel - 1) * 30 + Math.Max(0, (_playerEntity.TierLevel - 4) * 10);
                hitRate += 30 + (_playerEntity.TierLevel - 1) * 30 + Math.Max(0, (_playerEntity.TierLevel - 4) * 10);
                criticalChance += 3 + Math.Max(0, (_playerEntity.TierLevel - 4) * 2);
                criticalDamage += 18 + (_playerEntity.TierLevel - 1) + Math.Max(0, _playerEntity.TierLevel - 4);
                meleeDefense += 10 + (_playerEntity.TierLevel - 1) * 10;
                rangeDefense += 10 + (_playerEntity.TierLevel - 1) * 10;
                magicDefense += 20 + (_playerEntity.TierLevel - 1) * 20 + Math.Max(0, (_playerEntity.TierLevel - 4) * 10);
                meleeDodge += 30 + (_playerEntity.TierLevel - 1) * 40 - Math.Max(0, (_playerEntity.TierLevel - 3) * 10);
                fireResistance += 13 + (_playerEntity.TierLevel - 1);
                waterResistance += 13 + (_playerEntity.TierLevel - 1);
                lightResistance += 13 + (_playerEntity.TierLevel - 1);
                shadowResistance += 13 + (_playerEntity.TierLevel - 1);
                break;

            case SubClassType.Pyromancer:
                minDamage += 40 + (_playerEntity.TierLevel - 1) * 40;
                maxDamage += 40 + (_playerEntity.TierLevel - 1) * 40;
                hitRate += 30 + (_playerEntity.TierLevel - 1) * 30;
                criticalChance += 3 + (_playerEntity.TierLevel - 1);
                criticalDamage += 18 + (_playerEntity.TierLevel - 1);
                meleeDefense += 10 + (_playerEntity.TierLevel - 1) * 10;
                rangeDefense += 10 + (_playerEntity.TierLevel - 1) * 20;
                magicDefense += 20 + (_playerEntity.TierLevel - 1) * 20;
                meleeDodge += 30 + (_playerEntity.TierLevel - 1) * 40;
                fireResistance += 13 + (_playerEntity.TierLevel - 1);
                waterResistance += 13 + (_playerEntity.TierLevel - 1);
                lightResistance += 13 + (_playerEntity.TierLevel - 1);
                shadowResistance += 13 + (_playerEntity.TierLevel - 1);
                break;

            case SubClassType.DarkNecromancer:
                minDamage += 50 + (_playerEntity.TierLevel - 1) * 40;
                maxDamage += 50 + (_playerEntity.TierLevel - 1) * 40;
                hitRate += 30 + (_playerEntity.TierLevel - 1) * 40;
                criticalChance += 4 + (_playerEntity.TierLevel - 1);
                criticalDamage += 21 + (_playerEntity.TierLevel - 1);
                meleeDefense += 10 + (_playerEntity.TierLevel - 1) * 10;
                rangeDefense += 10 + (_playerEntity.TierLevel - 1) * 10;
                magicDefense += 30 + (_playerEntity.TierLevel - 1) * 30;
                meleeDodge += 30 + (_playerEntity.TierLevel - 1) * 30;
                fireResistance += 11 + (_playerEntity.TierLevel - 1);
                waterResistance += 11 + (_playerEntity.TierLevel - 1);
                lightResistance += 11 + (_playerEntity.TierLevel - 1);
                shadowResistance += 11 + (_playerEntity.TierLevel - 1);
                break;

            case SubClassType.ZenWarrior:
                minDamage += 40 + (_playerEntity.TierLevel - 1) * 40;
                maxDamage += 40 + (_playerEntity.TierLevel - 1) * 40;
                hitRate += 50 + (_playerEntity.TierLevel - 1) * 50;
                criticalChance += 5 + (_playerEntity.TierLevel - 1);
                criticalDamage += 27 + (_playerEntity.TierLevel - 1);
                meleeDefense += 20 + (_playerEntity.TierLevel - 1) * 20;
                rangeDefense += 20 + (_playerEntity.TierLevel - 1) * 10;
                magicDefense += 20 + (_playerEntity.TierLevel - 1) * 10;
                meleeDodge += 30 + (_playerEntity.TierLevel - 1) * 30;
                fireResistance += 8 + (_playerEntity.TierLevel - 1);
                waterResistance += 8 + (_playerEntity.TierLevel - 1);
                lightResistance += 8 + (_playerEntity.TierLevel - 1);
                shadowResistance += 8 + (_playerEntity.TierLevel - 1);
                break;

            case SubClassType.EmperorsBlade:
                minDamage += 50 + (_playerEntity.TierLevel - 1) * 50;
                maxDamage += 50 + (_playerEntity.TierLevel - 1) * 50;
                hitRate += 50 + (_playerEntity.TierLevel - 1) * 50;
                criticalChance += 5 + (_playerEntity.TierLevel - 1);
                criticalDamage += 27 + (_playerEntity.TierLevel - 1);
                meleeDefense += 20 + (_playerEntity.TierLevel - 1) * 20;
                rangeDefense += 20 + (_playerEntity.TierLevel - 1) * 10;
                magicDefense += 10 + (_playerEntity.TierLevel - 1) * 10;
                meleeDodge += 30 + (_playerEntity.TierLevel - 1) * 30;
                fireResistance += 11 + (_playerEntity.TierLevel - 1);
                waterResistance += 11 + (_playerEntity.TierLevel - 1);
                lightResistance += 11 + (_playerEntity.TierLevel - 1);
                shadowResistance += 11 + (_playerEntity.TierLevel - 1);
                break;

            case SubClassType.StealthShadow:
                minDamage += 40 + (_playerEntity.TierLevel - 1) * 40;
                maxDamage += 40 + (_playerEntity.TierLevel - 1) * 40;
                hitRate += 40 + (_playerEntity.TierLevel - 1) * 40;
                criticalChance += 8 + (_playerEntity.TierLevel - 1);
                criticalDamage += 32 + (_playerEntity.TierLevel - 1); 
                meleeDefense += 20 + (_playerEntity.TierLevel - 1) * 10;
                rangeDefense += 20 + (_playerEntity.TierLevel - 1) * 10; 
                magicDefense += 10 + (_playerEntity.TierLevel - 1) * 10;
                meleeDodge += 40 + (_playerEntity.TierLevel - 1) * 30;
                fireResistance += 8 + (_playerEntity.TierLevel - 1); 
                waterResistance += 8 + (_playerEntity.TierLevel - 1);
                lightResistance += 8 + (_playerEntity.TierLevel - 1); 
                shadowResistance += 8 + (_playerEntity.TierLevel - 1);
                break;
        }
        
        fireResistance += _playerEntity.HaveStaticBonus(StaticBonusType.OrderOfDiscipline) ? 5 : 0;
        waterResistance += _playerEntity.HaveStaticBonus(StaticBonusType.OrderOfDiscipline) ? 5 : 0;
        lightResistance += _playerEntity.HaveStaticBonus(StaticBonusType.OrderOfDiscipline) ? 5 : 0;
        shadowResistance += _playerEntity.HaveStaticBonus(StaticBonusType.OrderOfDiscipline) ? 5 : 0;
        
        _stats[Statistics.MIN_DAMAGE] = minDamage;
        _stats[Statistics.MAX_DAMAGE] = maxDamage;
        _stats[Statistics.HITRATE] = hitRate;
        _stats[Statistics.CRITICAL_CHANCE] = criticalChance;
        _stats[Statistics.CRITICAL_DAMAGE] = criticalDamage;
        _stats[Statistics.SECOND_MIN_DAMAGE] = secondMinDamage;
        _stats[Statistics.SECOND_MAX_DAMAGE] = secondMaxDamage;
        _stats[Statistics.SECOND_HITRATE] = secondHitRate;
        _stats[Statistics.SECOND_CRITICAL_CHANCE] = secondCriticalChance;
        _stats[Statistics.SECOND_CRITICAL_DAMAGE] = secondCriticalDamage;
        _stats[Statistics.MELEE_DEFENSE] = meleeDefense;
        _stats[Statistics.RANGE_DEFENSE] = rangeDefense;
        _stats[Statistics.MAGIC_DEFENSE] = magicDefense;
        _stats[Statistics.MELEE_DODGE] = meleeDodge;
        _stats[Statistics.RANGE_DODGE] = rangeDodge;
        _stats[Statistics.FIRE_RESISTANCE] = fireResistance;
        _stats[Statistics.WATER_RESISTANCE] = waterResistance;
        _stats[Statistics.LIGHT_RESISTANCE] = lightResistance;
        _stats[Statistics.SHADOW_RESISTANCE] = shadowResistance;
    }
}