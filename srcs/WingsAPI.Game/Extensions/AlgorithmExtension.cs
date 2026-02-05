using System;
using System.Collections.Generic;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.EntityStatistics;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Extensions;

public static class AlgorithmExtension
{
    private static readonly HashSet<EquipmentType> EquipmentTypes = new()
    {
        EquipmentType.Sp,
        EquipmentType.MainWeapon,
        EquipmentType.SecondaryWeapon,
        EquipmentType.Armor
    };

    private static readonly HashSet<StatisticType> StatsTypes = new()
    {
        StatisticType.HITRATE_MAGIC,
        StatisticType.FIRE,
        StatisticType.WATER,
        StatisticType.LIGHT,
        StatisticType.DARK
    };
    
    private static IBattleEntityAlgorithmService _algorithmBattleEntity => StaticBattleEntityAlgorithmService.Instance;

    public static byte GetSpeed(this IBattleEntity entity, int speed)
    {
        (int firstData, int secondData) moveBCard =
            entity.BCardComponent.GetAllBCardsInformation(BCardType.Move, (byte)AdditionalTypes.Move.MovementSpeedIncreased, entity.Level);
        (int firstData, int secondData) moveBCardNegated =
            entity.BCardComponent.GetAllBCardsInformation(BCardType.Move, (byte)AdditionalTypes.Move.MovementSpeedDecreased, entity.Level);
        (int firstData, int secondData) fourthGlacernonFamilyRaidBCard =
            entity.BCardComponent.GetAllBCardsInformation(BCardType.FourthGlacernonFamilyRaid, 
                (byte)AdditionalTypes.FourthGlacernonFamilyRaid.IncreaseMovementSpeedTick, entity.Level);
        (int firstData, int secondData) fourthGlacernonFamilyRaidBCardNegated =
            entity.BCardComponent.GetAllBCardsInformation(BCardType.FourthGlacernonFamilyRaid, 
                (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DecreaseMovementSpeedTick, entity.Level);

        int increaseSpeed = moveBCard.firstData;
        increaseSpeed -= moveBCardNegated.firstData;

        speed += increaseSpeed;
        
        if (entity.BCardDataComponent.IncreaseSpeedTick.HasValue && entity.BCardDataComponent.IncreaseSpeedTick.Value <= fourthGlacernonFamilyRaidBCard.secondData)
        {
            speed += entity.BCardDataComponent.IncreaseSpeedTick.Value;
        }

        if (entity.BCardDataComponent.DecreaseSpeedTick.HasValue && entity.BCardDataComponent.DecreaseSpeedTick.Value <= fourthGlacernonFamilyRaidBCardNegated.secondData)
        {
            speed -= entity.BCardDataComponent.DecreaseSpeedTick.Value;
        }

        switch (entity)
        {
            case IPlayerEntity character:
                switch (character.IsOnVehicle)
                {
                    case true:
                        return (byte)(character.VehicleSpeed + character.VehicleMapSpeed);
                    case false when character.Specialist != null && character.UseSp:
                        speed += character.Specialist.GameItem.Speed;
                        break;
                }

                if (character.IsInvisible())
                {
                    speed += character.BCardComponent.GetAllBCardsInformation(BCardType.Move, (byte)AdditionalTypes.Move.InvisibleMovement, entity.Level).firstData;
                    speed -= character.BCardComponent.GetAllBCardsInformation(BCardType.Move, (byte)AdditionalTypes.Move.InvisibleMovementNegated, entity.Level).firstData;
                }

                speed += IncreaseByMap(character);
                
                if (character.Session?.CurrentMapInstance is { IsPvp: true } && character.SubClass is SubClassType.CrimsonFury or SubClassType.SilentStalker)
                {
                    double speedIncreaseRate = character.TierLevel switch
                    {
                        1 => 0.05, // Increase speed by 5%
                        2 => 0.06, // Increase speed by 6%
                        3 => 0.07, // Increase speed by 7%
                        4 => 0.08, // Increase speed by 8%
                        5 => 0.09, // Increase speed by 9%
                        _ => 0     // No increase if the level is not specified
                    };
                    
                    speed += (int)Math.Ceiling(speed * speedIncreaseRate);
                }
                break;
            case IMateEntity mateEntity:
                if (mateEntity.CanPickUp)
                {
                    speed += 2;
                }

                if (mateEntity.Specialist != null && mateEntity.IsUsingSp)
                {
                    speed += mateEntity.Specialist.GameItem.Speed;
                }

                if (mateEntity.IsTeamMember)
                {
                    speed += 2;
                }

                if (mateEntity.MateType == MateType.Partner && mateEntity.Skin != 0)
                {
                    speed += 2;
                }

                if (mateEntity.Owner.HasBuff(BuffVnums.GUARDIAN_BLESS))
                {
                    speed += 2;
                }

                break;
        }

        double multiplier = 1 + (entity.BCardComponent.GetAllBCardsInformation(BCardType.Move, (byte)AdditionalTypes.Move.MoveSpeedIncreasedPercentage, entity.Level).firstData * 0.01
            - entity.BCardComponent.GetAllBCardsInformation(BCardType.Move, (byte)AdditionalTypes.Move.MoveSpeedDecreasedPercentage, entity.Level).firstData * 0.01);

        speed = (int)(speed * multiplier);

        if (speed < 0)
        {
            speed = 0;
        }

        return speed > 59 ? (byte)59 : (byte)speed;
    }

    private static int IncreaseByMap(IPlayerEntity character)
    {
        int increaseSpeed = 0;
        IMapInstance mapInstance = character.MapInstance;
        if (mapInstance == null)
        {
            return increaseSpeed;
        }

        if (mapInstance.MapInstanceType == MapInstanceType.RainbowBattle)
        {
            increaseSpeed += character.BCardComponent
                .GetAllBCardsInformation(BCardType.IncreaseDamageVersus, (byte)AdditionalTypes.IncreaseDamageVersus.PvpDamageAndSpeedRainbowBattleIncrease, character.Level).secondData;
        }

        return increaseSpeed;
    }

    public static int GetMaxHp(this IBattleEntity battleEntity, int baseHp)
    {
        int hp = baseHp;
        if (battleEntity is IPlayerEntity character)
        {
            hp += character.StatisticsComponent.Passives.GetValueOrDefault(PassiveType.HP);
            if (character.UseSp)
            {
                double multiplier = 1;
                int point = character.Specialist != null && character.UseSp ? character.SpecialistComponent.GetSlHp(character.Specialist.CurrentActiveSpecialistSlot) : 0;

                if (point <= 50)
                {
                    multiplier += point * 0.01;
                }
                else
                {
                    multiplier += 0.5 + (point * 0.01 - 0.5) * 2;
                }

                hp = (int)(hp * multiplier);

                hp += character.SpecialistComponent.Hp + character.SpecialistComponent.SpHP * 100;
            }
        }

        int increaseHp = 0;
        double hpMultiplier = 1;

        bool bearSpirit = battleEntity.BCardComponent.HasBCard(BCardType.BearSpirit, (byte)AdditionalTypes.BearSpirit.IncreaseMaximumHP);
        double bearMultiplier = battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.BearSpirit, (byte)AdditionalTypes.BearSpirit.IncreaseMaximumHP, battleEntity.Level).firstData * 0.01;
        if (bearSpirit)
        {
            int bearIncrease = (int)(hp * bearMultiplier);
            if (bearIncrease > 5000)
            {
                bearIncrease = 5000;
            }

            hp += bearIncrease;
        }

        bool bearSpirit2 = battleEntity.BCardComponent.HasBCard(BCardType.BearSpirit, (byte)AdditionalTypes.BearSpirit.DecreaseMaximumHP);
        double bearMultiplier2 = battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.BearSpirit, (byte)AdditionalTypes.BearSpirit.DecreaseMaximumHP, battleEntity.Level).firstData * 0.01;
        if (bearSpirit2)
        {
            int bearDecrease = (int)(hp * bearMultiplier2);
            if (bearDecrease > 5000)
            {
                bearDecrease = 5000;
            }

            hp -= bearDecrease;
        }

        hpMultiplier += battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.Recovery, (byte)AdditionalTypes.Recovery.HpPercentualIncreased, battleEntity.Level).firstData * 0.01;
        hpMultiplier += battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.IncreasesMaximumHP, battleEntity.Level).firstData * 0.01;
        hpMultiplier -= battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.DecreasesMaximumHP, battleEntity.Level).firstData * 0.01;
        hpMultiplier += battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.GetInDodge, (byte)AdditionalTypes.GetInDodge.IncreaseHpInPercent, battleEntity.Level).firstData * 0.01;
        hpMultiplier += battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumHPPercentIncreased, battleEntity.Level).firstData * 0.01;
        
        increaseHp += battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumHPIncreased, battleEntity.Level).firstData;
        increaseHp += battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumHPMPIncreased, battleEntity.Level).firstData;

        increaseHp -= battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumHPDecreased, battleEntity.Level).firstData;
        increaseHp -= battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumHPMPDecreased, battleEntity.Level).firstData;

        if (battleEntity is MateEntity { MateType: MateType.Partner, IsUsingSp: true } partner)
        {
            increaseHp += partner.Specialist.SpHP * 300;
        }
        
        int increaseFinalHp = (int)(hp * hpMultiplier);
        hp = increaseFinalHp + increaseHp;

        if (battleEntity is IPlayerEntity playerEntity)
        {
            hp += playerEntity.GetJewelsCellonsValue(CellonType.Hp);
        }

        return hp;
    }

    public static int GetMaxMp(this IBattleEntity battleEntity, int baseMp)
    {
        int mp = baseMp;

        if (battleEntity is IPlayerEntity character)
        {
            mp += character.StatisticsComponent.Passives.GetValueOrDefault(PassiveType.MP);

            if (character.UseSp)
            {
                double multiplier = 1;
                int point = character.SpecialistComponent.GetSlHp(character.Specialist.CurrentActiveSpecialistSlot);

                if (point <= 50)
                {
                    multiplier += point * 0.01;
                }
                else
                {
                    multiplier += 0.5 + (point * 0.01 - 0.5) * 2;
                }

                mp = (int)(mp * multiplier);

                mp += character.SpecialistComponent.Mp + character.SpecialistComponent.SpHP * 100;
            }
        }

        int increaseMp = 0;
        double mpMultiplier = 1;

        bool bearSpirit = battleEntity.BCardComponent.HasBCard(BCardType.BearSpirit, (byte)AdditionalTypes.BearSpirit.IncreaseMaximumMP);
        double bearMultiplier = battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.BearSpirit, (byte)AdditionalTypes.BearSpirit.IncreaseMaximumMP, battleEntity.Level).firstData * 0.01;
        if (bearSpirit)
        {
            int bearIncrease = (int)(mp * bearMultiplier);
            if (bearIncrease > 5000)
            {
                bearIncrease = 5000;
            }

            mp += bearIncrease;
        }
        
        bool bearSpirit2 = battleEntity.BCardComponent.HasBCard(BCardType.BearSpirit, (byte)AdditionalTypes.BearSpirit.DecreaseMaximumMP);
        double bearMultiplier2 = battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.BearSpirit, (byte)AdditionalTypes.BearSpirit.DecreaseMaximumMP, battleEntity.Level).firstData * 0.01;
        if (bearSpirit2)
        {
            int bearDecrease = (int)(mp * bearMultiplier2);
            if (bearDecrease > 5000)
            {
                bearDecrease = 5000;
            }

            mp -= bearDecrease;
        }

        mpMultiplier += battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.Recovery, (byte)AdditionalTypes.Recovery.MpPercentualIncreased, battleEntity.Level).firstData * 0.01;
        mpMultiplier += battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.IncreasesMaximumMP, battleEntity.Level).firstData * 0.01;
        mpMultiplier -= battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.DecreasesMaximumMP, battleEntity.Level).firstData * 0.01;
        mpMultiplier += battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.GetInDodge, (byte)AdditionalTypes.GetInDodge.IncreaseMpInPercent, battleEntity.Level).firstData * 0.01;
        mpMultiplier += battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumMPPercentIncreased, battleEntity.Level).firstData * 0.01;
        
        increaseMp += battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumMPIncreased, battleEntity.Level).firstData;
        increaseMp += battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumHPMPIncreased, battleEntity.Level).firstData;

        increaseMp -= battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumMPDecreased, battleEntity.Level).firstData;
        increaseMp -= battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumHPMPDecreased, battleEntity.Level).firstData;
        
        if (battleEntity is MateEntity { MateType: MateType.Partner, IsUsingSp: true } partner)
        {
            increaseMp += partner.Specialist.SpHP * 300;
        }
        
        int increaseFinalMp = (int)(mp * mpMultiplier);
        mp = increaseFinalMp + increaseMp;

        if (battleEntity is IPlayerEntity playerEntity)
        {
            mp += playerEntity.GetJewelsCellonsValue(CellonType.Mp);
        }

        return mp;
    }


    public static void RefreshMaxHpMp(this IBattleEntity entity, IBattleEntityAlgorithmService algorithm)
    {
        switch (entity)
        {
            case IPlayerEntity character:
                character.MaxHp = algorithm.GetBasicHpByClass(character.Class, character.Level);
                character.MaxMp = algorithm.GetBasicMpByClass(character.Class, character.Level);
                break;
            case IMateEntity mateEntity:
                mateEntity.MaxHp = algorithm.GetBasicHp((int)mateEntity.MonsterRaceType, mateEntity.Level, mateEntity.MeleeHpFactor, mateEntity.CleanHp, false) + 
                    algorithm.GetStarsHpMp(mateEntity.Stars, mateEntity.HeroLevel);
                mateEntity.MaxMp = algorithm.GetBasicMp((int)mateEntity.MonsterRaceType, mateEntity.Level, mateEntity.MagicMpFactor, mateEntity.CleanMp, false) + 
                    algorithm.GetStarsHpMp(mateEntity.Stars, mateEntity.HeroLevel);
                break;
        }
    }

    public static long GetLevelXp(this IPlayerEntity character, ICharacterAlgorithm characterAlgorithm) => characterAlgorithm.GetLevelXp(character.Level);
    public static int GetJobXp(this IPlayerEntity character, ICharacterAlgorithm characterAlgorithm, bool isAdventurer = false) => characterAlgorithm.GetJobXp(character.JobLevel, isAdventurer);
    public static int GetHeroXp(this IPlayerEntity character, ICharacterAlgorithm characterAlgorithm) => characterAlgorithm.GetHeroLevelXp(character.HeroLevel);

    public static int GetSpJobXp(this IPlayerEntity character, ICharacterAlgorithm characterAlgorithm, bool isFunSpecialist) =>
        characterAlgorithm.GetSpecialistJobXp(character.Specialist.SpLevel, isFunSpecialist);

    public static int GetResistance(this IPlayerEntity character, StatisticType type)
    {
        int resistance = 0;

        GameItemInstance specialistInstance = character.Specialist;
        if (character.UseSp && specialistInstance != null)
        {
            resistance += type switch
            {
                StatisticType.FIRE => character.SpecialistComponent.FireResistance + specialistInstance.GameItem.FireResistance + character.SpecialistComponent.SpFire,
                StatisticType.WATER => character.SpecialistComponent.WaterResistance + specialistInstance.GameItem.WaterResistance + character.SpecialistComponent.SpWater,
                StatisticType.LIGHT => character.SpecialistComponent.LightResistance + specialistInstance.GameItem.LightResistance + character.SpecialistComponent.SpLight,
                StatisticType.DARK => character.SpecialistComponent.DarkResistance + specialistInstance.GameItem.DarkResistance + character.SpecialistComponent.SpDark
            };
        }

        resistance += character.FindMoreStats(type);

        return resistance;
    }

    private static int FindMoreStats(this IPlayerEntity character, StatisticType type)
    {
        int stats = 0;

        IReadOnlyDictionary<PassiveType, int> passives = character.StatisticsComponent.Passives;

        stats += type switch
        {
            StatisticType.ATTACK_MELEE => passives.GetValueOrDefault(PassiveType.MELEE_ATTACK),
            StatisticType.ATTACK_RANGED => passives.GetValueOrDefault(PassiveType.RANGED_ATTACK),
            StatisticType.ATTACK_MAGIC => passives.GetValueOrDefault(PassiveType.MAGIC_ATTACK),
            StatisticType.HITRATE_MELEE => passives.GetValueOrDefault(PassiveType.MELEE_HIRATE),
            StatisticType.HITRATE_RANGED => passives.GetValueOrDefault(PassiveType.RANGED_HIRATE),
            StatisticType.DEFENSE_MELEE => passives.GetValueOrDefault(PassiveType.MELEE_DEFENCE),
            StatisticType.DEFENSE_RANGED => passives.GetValueOrDefault(PassiveType.RANGED_DEFENCE),
            StatisticType.DEFENSE_MAGIC => passives.GetValueOrDefault(PassiveType.MAGIC_DEFENCE),
            StatisticType.DODGE_MELEE => passives.GetValueOrDefault(PassiveType.MELEE_DODGE),
            StatisticType.DODGE_RANGED => passives.GetValueOrDefault(PassiveType.RANGED_DODGE),
            _ => 0
        };

        foreach (InventoryItem inventoryItem in character.EquippedItems)
        {
            GameItemInstance item = inventoryItem?.ItemInstance;

            if (item == null)
            {
                continue;
            }

            if (EquipmentTypes.Contains(item.GameItem.EquipmentSlot) && !StatsTypes.Contains(type))
            {
                continue;
            }

            if (item.Type == ItemInstanceType.SpecialistInstance)
            {
                continue;
            }

            stats += type switch
            {
                StatisticType.HITRATE_MELEE => item.HitRate + item.GameItem.HitRate,
                StatisticType.HITRATE_RANGED => item.HitRate + item.GameItem.HitRate,
                StatisticType.DEFENSE_MELEE => item.CloseDefence + item.GameItem.CloseDefence,
                StatisticType.DEFENSE_RANGED => item.DistanceDefence + item.GameItem.DistanceDefence,
                StatisticType.DEFENSE_MAGIC => item.MagicDefence + item.GameItem.MagicDefence,
                StatisticType.DODGE_MELEE => item.DefenceDodge + item.GameItem.DefenceDodge,
                StatisticType.DODGE_RANGED => item.DistanceDefenceDodge + item.GameItem.DistanceDefenceDodge,
                StatisticType.FIRE => item.FireResistance + item.GameItem.FireResistance,
                StatisticType.WATER => item.WaterResistance + item.GameItem.WaterResistance,
                StatisticType.LIGHT => item.LightResistance + item.GameItem.LightResistance,
                StatisticType.DARK => item.DarkResistance + item.GameItem.DarkResistance,
                _ => 0
            };
        }

        return stats;
    }

    public static int FindMoreStats(this IMateEntity mateEntity, StatisticType type)
    {
        int stats = 0;

        if (mateEntity.MateType == MateType.Pet)
        {
            stats += _algorithmBattleEntity.GetStarsAllResistances(mateEntity.Stars, mateEntity.HeroLevel);
            return stats;
        }

        foreach (PartnerInventoryItem inventoryItem in mateEntity.Owner.PartnerGetEquippedItems(mateEntity.PetSlot))
        {
            GameItemInstance item = inventoryItem?.ItemInstance;

            if (item == null)
            {
                continue;
            }

            if (item.GameItem.EquipmentSlot == EquipmentType.Sp && !mateEntity.IsUsingSp)
            {
                continue;
            }

            stats += type switch
            {
                StatisticType.HITRATE_MELEE => item.HitRate + item.GameItem.HitRate,
                StatisticType.HITRATE_RANGED => item.HitRate + item.GameItem.HitRate,
                StatisticType.DEFENSE_MELEE => item.CloseDefence + item.GameItem.CloseDefence,
                StatisticType.DEFENSE_RANGED => item.DistanceDefence + item.GameItem.DistanceDefence,
                StatisticType.DEFENSE_MAGIC => item.MagicDefence + item.GameItem.MagicDefence,
                StatisticType.DODGE_MELEE => item.DefenceDodge + item.GameItem.DefenceDodge,
                StatisticType.DODGE_RANGED => item.DistanceDefenceDodge + item.GameItem.DistanceDefenceDodge,
                StatisticType.FIRE => item.FireResistance + item.GameItem.FireResistance,
                StatisticType.WATER => item.WaterResistance + item.GameItem.WaterResistance,
                StatisticType.LIGHT => item.LightResistance + item.GameItem.LightResistance,
                StatisticType.DARK => item.DarkResistance + item.GameItem.DarkResistance,
                _ => 0
            };
        }
        
        if (mateEntity.Specialist?.SpStoneUpgrade > 0 && mateEntity.IsUsingSp)
        {
            stats += type switch
            {
                StatisticType.ATTACK_MELEE => mateEntity.Specialist.SpDamage * 20,
                StatisticType.ATTACK_MAGIC => mateEntity.Specialist.SpDamage * 20,
                StatisticType.ATTACK_RANGED => mateEntity.Specialist.SpDamage * 20,
                StatisticType.HITRATE_MAGIC => mateEntity.Specialist.SpDamage * 20,
                StatisticType.HITRATE_MELEE => mateEntity.Specialist.SpDamage * 20,
                StatisticType.HITRATE_RANGED => mateEntity.Specialist.SpDamage * 20,
                StatisticType.DEFENSE_MELEE => mateEntity.Specialist.SpDefence * 10,
                StatisticType.DEFENSE_RANGED => mateEntity.Specialist.SpDefence * 10,
                StatisticType.DEFENSE_MAGIC => mateEntity.Specialist.SpDefence * 10,
                StatisticType.FIRE => mateEntity.Specialist.SpFire,
                StatisticType.WATER => mateEntity.Specialist.SpWater,
                StatisticType.LIGHT => mateEntity.Specialist.SpLight,
                StatisticType.DARK => mateEntity.Specialist.SpDark,
                _ => 0
            };
        }

        return stats;
    }

    public static int GetHitRate(this IPlayerEntity character, int basic, bool isMainWeapon, StatisticType type)
    {
        int hitRate = basic;
        GameItemInstance mainWeapon = character.MainWeapon;
        GameItemInstance secondaryWeapon = character.SecondaryWeapon;

        if (character.UseSp && type != StatisticType.HITRATE_MAGIC)
        {
            hitRate += character.SpecialistComponent.HitRate;
        }

        hitRate += character.FindMoreStats(type);

        if (isMainWeapon)
        {
            if (mainWeapon == null)
            {
                return hitRate;
            }

            return hitRate + mainWeapon.HitRate + mainWeapon.GameItem.HitRate;
        }

        if (secondaryWeapon == null)
        {
            return hitRate;
        }

        return hitRate + secondaryWeapon.HitRate + secondaryWeapon.GameItem.HitRate;
    }

    public static int GetDodge(this IPlayerEntity character, int basic, StatisticType type)
    {
        int dodge = basic;
        GameItemInstance armorInstance = character.Armor;

        if (character.UseSp)
        {
            dodge += type switch
            {
                StatisticType.DODGE_MELEE => character.SpecialistComponent.DefenceDodge,
                StatisticType.DODGE_RANGED => character.SpecialistComponent.DistanceDefenceDodge
            };
        }

        dodge += character.FindMoreStats(type);

        if (armorInstance == null)
        {
            return dodge;
        }

        dodge += type switch
        {
            StatisticType.DODGE_MELEE => armorInstance.DefenceDodge + armorInstance.GameItem.DefenceDodge,
            StatisticType.DODGE_RANGED => armorInstance.DistanceDefenceDodge + armorInstance.GameItem.DistanceDefenceDodge
        };

        return dodge;
    }

    public static int GetCriticalChance(this IPlayerEntity character, int basic, bool isMainWeapon)
    {
        int criticalChance = basic;
        GameItemInstance mainWeapon = character.MainWeapon;
        GameItemInstance secondaryWeapon = character.SecondaryWeapon;

        if (character.UseSp)
        {
            criticalChance += character.SpecialistComponent.CriticalLuckRate;
        }

        if (isMainWeapon)
        {
            if (mainWeapon == null)
            {
                return criticalChance;
            }

            return criticalChance + mainWeapon.GameItem.CriticalLuckRate;
        }

        if (secondaryWeapon == null)
        {
            return criticalChance;
        }

        return criticalChance + secondaryWeapon.GameItem.CriticalLuckRate;
    }

    public static int GetCriticalDamage(this IPlayerEntity character, int basic, bool isMainWeapon)
    {
        int criticalDamage = basic;
        GameItemInstance mainWeapon = character.MainWeapon;
        GameItemInstance secondaryWeapon = character.SecondaryWeapon;

        if (character.UseSp)
        {
            criticalDamage += character.SpecialistComponent.CriticalRate;
        }

        if (isMainWeapon)
        {
            if (mainWeapon == null)
            {
                return criticalDamage;
            }

            return criticalDamage + mainWeapon.GameItem.CriticalRate;
        }

        if (secondaryWeapon == null)
        {
            return criticalDamage;
        }

        return criticalDamage + secondaryWeapon.GameItem.CriticalRate;
    }
    
    public static int GetElement(this IPlayerEntity character, bool isSpecialistRate)
    {
        int element = 0;
        GameItemInstance specialist = character.Specialist;
        
        if (!isSpecialistRate)
        {
            bool isBuffed = character.HasBuff(BuffVnums.FAIRY_BOOSTER);
            GameItemInstance fairy = character.Fairy;
            
            if (fairy == null)
            {
                return element;
            }
            
            element += character.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseElementFairy, (byte)AdditionalTypes.IncreaseElementFairy.FairyElementIncrease, character.Level).firstData;
            element += character.BCardComponent.GetAllBCardsInformation(BCardType.AbsorbedSpirit, (byte)AdditionalTypes.AbsorbedSpirit.IncreaseFairyElementIfActive, character.Level).firstData;
            element -= character.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseElementFairy, (byte)AdditionalTypes.IncreaseElementFairy.FairyElementDecrease, character.Level).firstData;
            element += (character.Family?.UpgradeValues.GetValueOrDefault(FamilyUpgradeType.FAIRY_ELEMENT_BOOST).Item1 ?? 0);

            if (character.BCardComponent.HasBCard(BCardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.IncreaseFairyElementByDebuffCount))
            {
                int badBuffCount = character.BuffComponent.GetAllBuffs(s => s.BuffGroup == BuffGroup.Bad).Count;
                element += badBuffCount;
            }
            
            if (character.BCardComponent.HasBCard(BCardType.AbsorbedSpirit, (byte)AdditionalTypes.AbsorbedSpirit.IncreaseFairyElementIfActive))
            {
                (int firstData, int secondData) fairyIncrease = character.BCardComponent.GetAllBCardsInformation(BCardType.AbsorbedSpirit, (byte)AdditionalTypes.AbsorbedSpirit.IncreaseFairyElementIfActive, character.Level);
                
                if (character.BuffComponent.HasBuff(fairyIncrease.secondData))
                {
                    element += fairyIncrease.firstData;
                }
            }
            
            int elementalBonus = character.SubClass == SubClassType.Pyromancer ? 5 + Math.Max(0, Math.Min(character.TierLevel - 1, 4)) : 0;
            
            int finalElement = element + fairy.ElementRate + fairy.GameItem.ElementRate + (isBuffed ? 30 : 0) + elementalBonus;
            
            return finalElement < 0 ? 0 : finalElement;
        }
        
        if (!character.UseSp || specialist == null)
        {
            return element;
        }
        
        int point = character.SpecialistComponent.GetSlElement(character.Specialist.CurrentActiveSpecialistSlot);
        int p;
        
        if (point <= 50)
        {
            p = point;
        }
        else
        {
            p = 50 + (point - 50) * 2;
        }
        
        return p + character.SpecialistComponent.SpElement;
    }

    public static int GetDefence(this IPlayerEntity character, int basic, StatisticType statisticType)
    {
        int armor = basic;
        GameItemInstance armorInstance = character.Armor;
        GameItemInstance specialist = character.Specialist;

        if (character.UseSp)
        {
            int point = character.SpecialistComponent.GetSlDefense(character.Specialist.CurrentActiveSpecialistSlot);

            int p = point switch
            {
                <= 10 => point,
                <= 20 => 10 + (point - 10) * 2,
                <= 30 => 30 + (point - 20) * 3,
                <= 40 => 60 + (point - 30) * 4,
                <= 50 => 100 + (point - 40) * 5,
                <= 60 => 150 + (point - 50) * 6,
                <= 70 => 210 + (point - 60) * 7,
                <= 80 => 280 + (point - 70) * 8,
                <= 90 => 360 + (point - 80) * 9,
                <= 100 => 450 + (point - 90) * 10,
                <= 115 => 660 + (point - 110) * 14,
                <= 119 => 730 + (point - 115) * 15,
                120 => 810,
                _ => 0
            };

            armor += character.SpecialistComponent.SpDefence * 10;
            switch (statisticType)
            {
                case StatisticType.DEFENSE_MELEE:
                    armor += character.SpecialistComponent.CloseDefence;
                    break;
                case StatisticType.DEFENSE_RANGED:
                    armor += character.SpecialistComponent.DistanceDefence;
                    break;
                case StatisticType.DEFENSE_MAGIC:
                    armor += character.SpecialistComponent.MagicDefence;
                    break;
            }

            armor += p;
        }

        armor += character.FindMoreStats(statisticType);

        if (armorInstance == null)
        {
            return armor;
        }

        switch (statisticType)
        {
            case StatisticType.DEFENSE_MELEE:
                return armor + armorInstance.CloseDefence + armorInstance.GameItem.CloseDefence;
            case StatisticType.DEFENSE_RANGED:
                return armor + armorInstance.DistanceDefence + armorInstance.GameItem.DistanceDefence;
            case StatisticType.DEFENSE_MAGIC:
                return armor + armorInstance.MagicDefence + armorInstance.GameItem.MagicDefence;
        }

        return armor;
    }

    public static int GetDamage(this IPlayerEntity character, int basic, bool isMin, StatisticType type, bool isMainWeapon = true)
    {
        int damage = basic;
        GameItemInstance mainWeapon = character.MainWeapon;
        GameItemInstance secondaryWeapon = character.SecondaryWeapon;

        if (character.UseSp)
        {
            int point = character.SpecialistComponent.GetSlHit(character.Specialist.CurrentActiveSpecialistSlot);

            int p = point switch
            {
                <= 10 => point * 5,
                <= 20 => 50 + (point - 10) * 6,
                <= 30 => 110 + (point - 20) * 7,
                <= 40 => 180 + (point - 30) * 8,
                <= 50 => 260 + (point - 40) * 9,
                <= 60 => 350 + (point - 50) * 10,
                <= 70 => 450 + (point - 60) * 11,
                <= 80 => 560 + (point - 70) * 13,
                <= 90 => 690 + (point - 80) * 14,
                <= 94 => 830 + (point - 90) * 15,
                <= 95 => 890 + 16,
                <= 97 => 906 + (point - 95) * 17,
                <= 100 => 940 + (point - 97) * 20,
                <= 110 => 1000 + (point - 100) * 15,
                <= 120 => 1150 + (point - 110) * 20,
                _ => 0
            };

            damage += p;
            damage += character.SpecialistComponent.SpDamage * 10;
            if (isMin)
            {
                damage += character.SpecialistComponent.DamageMinimum;
            }
            else
            {
                damage += character.SpecialistComponent.DamageMaximum;
            }
        }

        damage += character.FindMoreStats(type);

        if (isMainWeapon)
        {
            if (mainWeapon == null)
            {
                return damage;
            }

            if (isMin)
            {
                return damage + mainWeapon.DamageMinimum + mainWeapon.GameItem.DamageMinimum;
            }

            return damage + mainWeapon.DamageMaximum + mainWeapon.GameItem.DamageMaximum;
        }

        if (secondaryWeapon == null)
        {
            return damage;
        }

        if (isMin)
        {
            return damage + secondaryWeapon.DamageMinimum + secondaryWeapon.GameItem.DamageMinimum;
        }

        return damage + secondaryWeapon.DamageMaximum + secondaryWeapon.GameItem.DamageMaximum;
    }

    public static int SlPoint(this GameItemInstance specialistInstance, short spPoint, SpecialistPointsType type)
    {
        try
        {
            int point = 0;
            switch (type)
            {
                case SpecialistPointsType.ATTACK:
                    point = spPoint switch
                    {
                        <= 10 => spPoint,
                        <= 28 => 10 + (spPoint - 10) / 2,
                        <= 88 => 19 + (spPoint - 28) / 3,
                        <= 168 => 39 + (spPoint - 88) / 4,
                        <= 268 => 59 + (spPoint - 168) / 5,
                        <= 334 => 79 + (spPoint - 268) / 6,
                        <= 383 => 90 + (spPoint - 334) / 7,
                        <= 391 => 97 + (spPoint - 383) / 8,
                        <= 400 => 98 + (spPoint - 391) / 9,
                        <= 410 => 99 + (spPoint - 400) / 10,
                        <= 425 => 100 + (spPoint - 410) / 3,
                        <= 481 => 105 + (spPoint - 425) / 4,
                        <= 486 => 119 + (spPoint - 481) / 5,
                        _ => point
                    };

                    break;

                case SpecialistPointsType.DEFENCE:
                    point = spPoint switch
                    {
                        <= 10 => spPoint,
                        <= 48 => 10 + (spPoint - 10) / 2,
                        <= 81 => 29 + (spPoint - 48) / 3,
                        <= 161 => 40 + (spPoint - 81) / 4,
                        <= 236 => 60 + (spPoint - 161) / 5,
                        <= 290 => 75 + (spPoint - 236) / 6,
                        <= 360 => 84 + (spPoint - 290) / 7,
                        <= 400 => 97 + (spPoint - 360) / 8,
                        <= 410 => 99 + (spPoint - 400) / 10,
                        <= 425 => 100 + (spPoint - 410) / 3,
                        <= 481 => 105 + (spPoint - 425) / 4,
                        <= 486 => 119 + (spPoint - 481) / 5,
                        _ => point
                    };

                    break;

                case SpecialistPointsType.ELEMENT:
                    point = spPoint switch
                    {
                        <= 20 => spPoint,
                        <= 40 => 20 + (spPoint - 20) / 2,
                        <= 70 => 30 + (spPoint - 40) / 3,
                        <= 110 => 40 + (spPoint - 70) / 4,
                        <= 210 => 50 + (spPoint - 110) / 5,
                        <= 270 => 70 + (spPoint - 210) / 6,
                        <= 410 => 80 + (spPoint - 270) / 7,
                        <= 425 => 100 + (spPoint - 410) / 3,
                        <= 481 => 105 + (spPoint - 425) / 4,
                        <= 486 => 119 + (spPoint - 481) / 5,
                        _ => point
                    };

                    break;

                case SpecialistPointsType.HPMP:
                    point = spPoint switch
                    {
                        <= 10 => spPoint,
                        <= 50 => 10 + (spPoint - 10) / 2,
                        <= 110 => 30 + (spPoint - 50) / 3,
                        <= 150 => 50 + (spPoint - 110) / 4,
                        <= 200 => 60 + (spPoint - 150) / 5,
                        <= 260 => 70 + (spPoint - 200) / 6,
                        <= 330 => 80 + (spPoint - 260) / 7,
                        <= 410 => 90 + (spPoint - 330) / 8,
                        <= 425 => 100 + (spPoint - 410) / 3,
                        <= 481 => 105 + (spPoint - 425) / 4,
                        <= 486 => 119 + (spPoint - 481) / 5,
                        _ => point
                    };

                    break;
            }

            return point;
        }
        catch
        {
            return 0;
        }
    }

    public static int SpPointsBasic(this GameItemInstance specialistInstance)
    {
        short spLevel = specialistInstance.SpLevel;
        int point = (spLevel - 20) * 3;
        if (spLevel <= 20)
        {
            point = 0;
        }

        switch (specialistInstance.Upgrade)
        {
            case 1:
                point += 5;
                break;

            case 2:
                point += 10;
                break;

            case 3:
                point += 15;
                break;

            case 4:
                point += 20;
                break;

            case 5:
                point += 28;
                break;

            case 6:
                point += 36;
                break;

            case 7:
                point += 46;
                break;

            case 8:
                point += 56;
                break;

            case 9:
                point += 68;
                break;

            case 10:
                point += 80;
                break;

            case 11:
                point += 95;
                break;

            case 12:
                point += 110;
                break;

            case 13:
                point += 128;
                break;

            case 14:
                point += 148;
                break;

            case 15:
                point += 173;
                break;
            
            case 16:
                point += 188;
                break;

            case 17:
                point += 203;
                break;

            case 18:
                point += 218;
                break;

            case 19:
                point += 233;
                break;

            case 20:
                point += 249;
                break;
        }

        return point;
    }
}