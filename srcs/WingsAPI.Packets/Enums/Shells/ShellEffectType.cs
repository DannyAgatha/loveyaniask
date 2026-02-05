namespace WingsAPI.Packets.Enums.Shells
{
    public enum ShellEffectType : byte
    {
        /* WEAPON OPTIONS */

        // INCREASE DAMAGES
        DamageImproved = 1,
        PercentageTotalDamage = 2,

        // DEBUFFS
        MinorBleeding = 3,
        Bleeding = 4,
        HeavyBleeding = 5,
        Blackout = 6,
        Freeze = 7,
        DeadlyBlackout = 8,

        // PVE OPTIONS
        DamageIncreasedPlant = 9,
        DamageIncreasedAnimal = 10,
        DamageIncreasedEnemy = 11,
        DamageIncreasedUnDead = 12,
        DamageIncreasedSmallMonster = 13,
        DamageIncreasedBigMonster = 14,

        // CHARACTER BONUSES
        CriticalChance = 15, //Except Staff
        CriticalDamage = 16, //Except Staff
        AntiMagicDisorder = 17, //Only Staff
        IncreasedFireProperties = 18,
        IncreasedWaterProperties = 19,
        IncreasedLightProperties = 20,
        IncreasedDarkProperties = 21,
        IncreasedElementalProperties = 22,
        ReducedMPConsume = 23,
        HPRecoveryForKilling = 24,
        MPRecoveryForKilling = 25,

        // SP BONUSES
        SLDamage = 26,
        SLDefence = 27,
        SLElement = 28,
        SLHP = 29,
        SLGlobal = 30,

        // PVE RATES INCREASE
        GainMoreGold = 31,
        GainMoreXP = 32,
        GainMoreCXP = 33,

        // PVP OPTIONS
        PercentageDamageInPVP = 34,
        ReducesPercentageEnemyDefenceInPVP = 35,
        ReducesEnemyFireResistanceInPVP = 36,
        ReducesEnemyWaterResistanceInPVP = 37,
        ReducesEnemyLightResistanceInPVP = 38,
        ReducesEnemyDarkResistanceInPVP = 39,
        ReducesEnemyAllResistancesInPVP = 40,
        NeverMissInPVP = 41,
        PVPDamageAt15Percent = 42,
        ReducesEnemyMPInPVP = 43,

        // R8 CHAMPION OPTIONS
        InspireFireResistanceWithPercentage = 44,
        InspireWaterResistanceWithPercentage = 45,
        InspireLightResistanceWithPercentage = 46,
        InspireDarkResistanceWithPercentage = 47,
        GainSPForKilling = 48,
        IncreasedPrecision = 49,
        IncreasedFocus = 50,

        /* ARMOR OPTIONS */

        // DEFENSE INCREASE
        CloseDefence = 51,
        DistanceDefence = 52,
        MagicDefence = 53,
        PercentageTotalDefence = 54,

        // ANTI-DEBUFFS
        ReducedMinorBleeding = 55,
        ReducedBleedingAndMinorBleeding = 56,
        ReducedAllBleedingType = 57,
        ReducedStun = 58,
        ReducedAllStun = 59,
        ReducedParalysis = 60,
        ReducedFreeze = 61,
        ReducedBlind = 62,
        ReducedSlow = 63,
        ReducedArmorDeBuff = 64,
        ReducedShock = 65,
        ReducedPoisonParalysis = 66,
        ReducedAllNegativeEffect = 67,

        // CHARACTER BONUSES
        RecovryHPOnRest = 68,
        RevoryHP = 69,
        RecoveryMPOnRest = 70,
        RecoveryMP = 71,
        RecoveryHPInDefence = 72,
        ReducedCritChanceRecive = 73,

        // RESISTANCE INCREASE
        IncreasedFireResistance = 74,
        IncreasedWaterResistance = 75,
        IncreasedLightResistance = 76,
        IncreasedDarkResistance = 77,
        IncreasedAllResistance = 78,

        // VARIOUS PVE BONUSES
        ReducedPrideLoss = 79,
        ReducedProductionPointConsumed = 80,
        IncreasedProductionPossibility = 81,
        IncreasedRecoveryItemSpeed = 82,

        // PVP BONUSES
        PercentageAllPVPDefence = 83,
        CloseDefenceDodgeInPVP = 84,
        DistanceDefenceDodgeInPVP = 85,
        IgnoreMagicDamage = 86,
        DodgeAllDamage = 87,
        ProtectMPInPVP = 88,

        // R8 CHAMPION OPTIONS
        FireDamageImmuneInPVP = 89,
        WaterDamageImmuneInPVP = 90,
        LightDamageImmuneInPVP = 91,
        DarkDamageImmuneInPVP = 92,
        AbsorbDamagePercentageA = 93,
        AbsorbDamagePercentageB = 94,
        AbsorbDamagePercentageC = 95,
        IncreaseEvasiveness = 96
    }
}