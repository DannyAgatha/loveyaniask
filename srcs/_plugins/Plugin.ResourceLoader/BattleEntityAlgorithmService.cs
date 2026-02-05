using System;
using WingsEmu.Game._enum;
using WingsEmu.Game.Algorithm;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Character;

namespace Plugin.ResourceLoader
{
    /*
     * Coefficient of statistics for the class:
     * Adventurer: 0, 0, 0
     * Swordsman: 8, 2, 0
     * Archer: 3, 6, 1
     * Mage: 0, 2, 8
     * Martial Artist: 5, 3, 2
     *
     * Additional statistics:
     * Adventurer: 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
     * Swordsman: 0, 15, 0, 0, 0, 0, 0, 0, 0, 0
     * Archer: 0, 15, 50, 15, 0, 0, 10, 0, 10, 0
     * Magician: 0, 0, 0, 0, 0, 0, 10, 20, 0, 0
     * Martial Artist: 0, 0, 20, 0, 0, 0, 20, 10, 10, 0
     */
    public class BattleEntityAlgorithmService : IBattleEntityAlgorithmService
    {
        private static readonly double[] DefenseRace0 = { 16, 13.5, 11, 50, 50, 50 };
        private static readonly double[] DefenseRace1 = { 20, 17, 19, 100, 100, 100 };
        private static readonly double[] DefenseRace2 = { 15, 15, 15, 75, 50, 40 };
        private static readonly double[] DefenseRace3 = { 15, 15, 15, 50, 50, 50 };
        private static readonly double[] DefenseRace4 = { 17.4, 17.4, 17.4, 60, 60, 100 };
        private static readonly double[] DefenseRace5 = { 13.4, 13.4, 13.4, 40, 40, 40 };
        private static readonly double[] DefenseRace6 = { 11.5, 15, 25, 50, 50, 75 };
        private static readonly double[] DefenseRace8 = { 10, 10, 10, 100, 100, 100 };
        private static readonly double[] DefaultDefense = { 0, 0, 0, 0, 0, 0 };

        private static readonly int[,] Statistics =
        {
            { 0, 0, 0 },
            { 8, 2, 0 },
            { 3, 6, 1 },
            { 0, 2, 8 },
            { 5, 3, 2 }
        };

        private static readonly int[,] AdditionalStatistics =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 15, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 15, 50, 15, 0, 0, 10, 0, 10, 0 },
            { 0, 0, 0, 0, 0, 0, 10, 20, 0, 0 },
            { 0, 0, 20, 0, 0, 0, 20, 10, 10, 0 }
        };

        public int GetBasicHp(int race, int level, int modifier, int additionalHp = 0, bool isMonster = true)
        {
            double hp = 0;
            int a = 0;
            int b = 0;
            int c = 0;

            if (isMonster)
            {
                modifier = 0;
            }

            switch (race)
            {
                case 0:
                    a = 0;
                    b = 2;
                    c = 138;
                    break;
                case 1:
                    a = 10;
                    b = 10;
                    c = 610;
                    break;
                case 2:
                    a = 5;
                    b = 0;
                    c = 105;
                    break;
                case 3:
                    a = 0;
                    b = 0;
                    c = 205;
                    break;
                case 4:
                    a = 2;
                    b = 5;
                    c = 695;
                    break;
                case 5:
                    a = -2;
                    b = -3;
                    c = 263;
                    break;
                case 6:
                    a = 0;
                    b = -7;
                    c = 21;
                    break;
                default:
                    a = 0;
                    b = 0;
                    c = 0;
                    break;
            }

            if (race == 8)
            {
                hp = 7;
            }
            else
            {
                int x = level;
                if ((modifier + a) != 0)
                {
                    x += (int)Math.Floor((level - 1) / (decimal)(10.0 / (modifier + a)));
                }

                hp = 0.5 * (x * x) + (15.5 + b) * x + c;
            }

            if (!isMonster)
            {
                return (int)Math.Floor(hp + additionalHp);
            }

            switch (level)
            {
                case >= 37 and <= 51:
                    hp *= 1.2;
                    break;
                case >= 52 and <= 61:
                    hp *= 1.5;
                    break;
                case >= 62 and <= 71:
                    hp *= 1.8;
                    break;
                case >= 72 and <= 81:
                    hp *= 2.5;
                    break;
                case >= 81:
                    hp *= 3.5;
                    break;
            }

            return (int)Math.Floor(hp + additionalHp);
        }

        public int GetBaseStatistic(int level, ClassType classType, StatisticType statisticType)
        {
            switch (statisticType)
            {
                case StatisticType.ATTACK_MELEE:
                    return (int)(level + 9 + Math.Floor((decimal)((level - 1) * Statistics[(int)classType, 0] / 10.0))) + AdditionalStatistics[(int)classType, 0];
                case StatisticType.ATTACK_RANGED:
                    return (int)(level + 9 + Math.Floor((decimal)((level - 1) * Statistics[(int)classType, 1] / 10.0))) + AdditionalStatistics[(int)classType, 2];
                case StatisticType.ATTACK_MAGIC:
                    return (int)(level + 9 + Math.Floor((decimal)((level - 1) * Statistics[(int)classType, 2] / 10.0))) + AdditionalStatistics[(int)classType, 4];
                case StatisticType.HITRATE_MELEE:
                    return (int)(level + 9 + Math.Floor((decimal)((level - 1) * Statistics[(int)classType, 1] / 10.0))) + AdditionalStatistics[(int)classType, 1];
                case StatisticType.HITRATE_RANGED:
                    return (int)((level + 9 + Math.Floor((decimal)((level - 1) * Statistics[(int)classType, 1] / 10.0))) * 2) + AdditionalStatistics[(int)classType, 3];
                case StatisticType.DEFENSE_MELEE:
                    return (int)((level + 9 + Math.Floor((decimal)((level - 1) * Statistics[(int)classType, 0] / 10.0))) * (decimal)0.5) + AdditionalStatistics[(int)classType, 5];
                case StatisticType.DEFENSE_RANGED:
                    return (int)((level + 9 + Math.Floor((decimal)((level - 1) * Statistics[(int)classType, 1] / 10.0))) * (decimal)0.5) + AdditionalStatistics[(int)classType, 7];
                case StatisticType.DEFENSE_MAGIC:
                    return (int)((level + 9 + Math.Floor((decimal)((level - 1) * Statistics[(int)classType, 2] / 10.0))) * (decimal)0.5) + AdditionalStatistics[(int)classType, 9];
                case StatisticType.DODGE_MELEE:
                    return (int)(level + 9 + Math.Floor((decimal)((level - 1) * Statistics[(int)classType, 1] / 10.0))) + AdditionalStatistics[(int)classType, 6];
                case StatisticType.DODGE_RANGED:
                    return (int)(level + 9 + Math.Floor((decimal)((level - 1) * Statistics[(int)classType, 1] / 10.0))) + AdditionalStatistics[(int)classType, 8];
            }

            return 0;
        }

        public int GetBasicMp(int race, int level, int modifier, int additionalMp = 0, bool isMonster = true)
        {
            double mp = 0;
            int z = 0;
            double d = 0;
            int a = 0;
            int g = 0;
            int x = 0;

            if (isMonster)
            {
                modifier = 0;
            }

            switch (race)
            {
                case 0:
                    d = 4.75;
                    a = 0;
                    g = 0;
                    break;
                case 1:
                    d = 178.75;
                    a = 10;
                    g = 8;
                    break;
                case 2:
                    d = 50.75;
                    a = -2;
                    g = 4;
                    break;
                case 3:
                    d = 50.75;
                    a = 0;
                    g = 4;
                    break;
                case 4:
                    d = 385.75;
                    a = 10;
                    g = 6;
                    z = 1;
                    break;
                case 5:
                    d = 23.75;
                    a = -2;
                    g = 2;
                    z = 1;
                    break;
                case 6:
                    d = 705.75;
                    a = 5;
                    g = 14;
                    break;
                case 8:
                    return (int)Math.Floor(4 + (double)additionalMp);
                default:
                    d = 0;
                    a = 0;
                    g = 0;
                    break;
            }

            x = level;
            if ((modifier + a) != 0)
            {
                x += (int)Math.Floor((level - 1) / (decimal)(10.0 / (modifier + a)) + z);
            }

            mp = Math.Floor((5.25 + g) * x + d) + (Math.Floor((double)(x - 6) / 4) + 1) * 2 * (Mod(x - 2, 4) + 1 + Math.Floor(((double)x - 6) / 4) * 2);

            return (int)Math.Floor(mp + additionalMp);
        }

        public int GetBasicHpByClass(ClassType classType, int level)
        {
            int hp = classType switch
            {
                ClassType.Adventurer => GetBasicHp(3, level, 0, 0, false),
                ClassType.Swordman => GetBasicHp(3, level, 8, 0, false),
                ClassType.Archer => GetBasicHp(3, level, 3, 0, false),
                ClassType.Magician => GetBasicHp(3, level, 0, 0, false),
                ClassType.MartialArtist => GetBasicHp(3, level, 5, 0, false)
            };

            return hp;
        }

        public int GetBasicMpByClass(ClassType classType, int level)
        {
            int mp = classType switch
            {
                ClassType.Adventurer => GetBasicMp(3, level, 0, 0, false),
                ClassType.Swordman => GetBasicMp(3, level, 0, 0, false),
                ClassType.Archer => GetBasicMp(3, level, 1, 0, false),
                ClassType.Magician => GetBasicMp(3, level, 8, 0, false),
                ClassType.MartialArtist => GetBasicMp(3, level, 2, 0, false)
            };

            return mp;
        }

        public int GetAttack(bool isMin, int race, AttackType attackType, short weaponLevel, byte wInfo, short level, int modifier, int additional, bool isWild = true, short petLevel = 0,
            MateType mateType = MateType.Pet)
        {
            int calcLevel;
            int weaponMod;
            int a;
            int b;
            int c;

            if (isWild)
            {
                modifier = 0;
                calcLevel = level;
                weaponMod = weaponLevel;
            }
            else
            {
                calcLevel = petLevel;
                weaponMod = petLevel + level - weaponLevel;
            }

            switch (attackType)
            {
                case AttackType.Melee:
                    switch (race)
                    {
                        case 0:
                            a = 35;
                            b = 0;
                            break;
                        case 1:
                            a = 43;
                            b = 10;
                            break;
                        case 2:
                            a = 33;
                            b = 5;
                            break;
                        case 3:
                            a = 33;
                            b = 0;
                            break;
                        case 4:
                            a = 38;
                            b = 2;
                            break;
                        case 5:
                            a = 30;
                            b = -2;
                            break;
                        case 6:
                            a = 26;
                            b = 0;
                            break;
                        case 8:
                            a = 23;
                            b = 0;
                            break;
                        default:
                            a = 0;
                            b = 0;
                            break;
                    }

                    break;
                case AttackType.Ranged:
                    switch (race)
                    {
                        case 0:
                            a = 30;
                            b = 0;
                            break;
                        case 1:
                            a = 38;
                            b = 10;
                            break;
                        case 2:
                            a = 33;
                            b = 0;
                            break;
                        case 3:
                            a = 33;
                            b = 0;
                            break;
                        case 4:
                            a = 38;
                            b = 2;
                            break;
                        case 5:
                            a = 30;
                            b = -2;
                            break;
                        case 6:
                            a = 33;
                            b = 0;
                            break;
                        case 8:
                            a = 23;
                            b = 0;
                            break;
                        default:
                            a = 0;
                            b = 0;
                            break;
                    }

                    break;
                case AttackType.Magical:
                    switch (race)
                    {
                        case 0:
                            a = 25;
                            b = 0;
                            break;
                        case 1:
                            a = 41;
                            b = 10;
                            break;
                        case 2:
                            a = 33;
                            b = -2;
                            break;
                        case 3:
                            a = 33;
                            b = 0;
                            break;
                        case 4:
                            a = 38;
                            b = 10;
                            break;
                        case 5:
                            a = 30;
                            b = -2;
                            break;
                        case 6:
                            a = 53;
                            b = 5;
                            break;
                        case 8:
                            a = 23;
                            b = 0;
                            break;
                        default:
                            a = 0;
                            b = 0;
                            break;
                    }

                    break;
                default:
                    a = 0;
                    b = 0;
                    break;
            }

            if (wInfo > 1)
            {
                c = (int)Math.Floor((decimal)((calcLevel + 2.0) / (wInfo - 1))) + 1;
            }
            else
            {
                c = 0;
            }

            if (!isWild && mateType == MateType.Partner)
            {
                return calcLevel + 9 + (int)Math.Floor((calcLevel - 1) * (modifier / 10.0));
            }

            if (isMin)
            {
                return (int)Math.Floor(calcLevel + (a - 7.2) + 3.2 * weaponMod + Math.Floor((calcLevel - 1) * ((modifier + b) / 10.0)) + additional + c);
            }

            return (int)Math.Floor(calcLevel + a + 4.8 * weaponMod + Math.Floor((calcLevel - 1) * ((modifier + b) / 10.0)) + additional - c);
        }

        public int GetHitrate(int race, AttackType attackType, short weaponLevel, short level, int modifier, int additional, bool isWild = true, short petLevel = 0, MateType mateType = MateType.Pet)
        {
            int calcLevel;
            int weaponMod;
            int a;
            int b;

            if (isWild)
            {
                modifier = 0;
                calcLevel = level;
                weaponMod = weaponLevel;
            }
            else
            {
                calcLevel = petLevel;
                weaponMod = petLevel + level - weaponLevel;
            }

            if (!isWild && mateType == MateType.Partner)
            {
                switch (attackType)
                {
                    case AttackType.Melee:
                        return calcLevel + 9 + (int)Math.Floor((calcLevel - 1) * (modifier / 10.0));
                    case AttackType.Ranged:
                        return 2 * (calcLevel + 9 + (int)Math.Floor((calcLevel - 1) * (modifier / 10.0)));
                    default:
                        return 0;
                }
            }

            switch (attackType)
            {
                case AttackType.Melee:
                    switch (race)
                    {
                        case 0:
                            a = 22;
                            b = 0;
                            break;
                        case 1:
                            a = 30;
                            b = 10;
                            break;
                        case 2:
                            a = 25;
                            b = 0;
                            break;
                        case 3:
                            a = 25;
                            b = 0;
                            break;
                        case 4:
                            a = 30;
                            b = 2;
                            break;
                        case 5:
                            a = 22;
                            b = -2;
                            break;
                        case 6:
                            a = 25;
                            b = 0;
                            break;
                        case 8:
                            a = 15;
                            b = 0;
                            break;
                        default:
                            a = 0;
                            b = 0;
                            break;
                    }

                    return (int)Math.Floor(calcLevel + 4 * weaponMod + a + Math.Floor((calcLevel - 1) * ((modifier + b) / 10.0)) + additional);

                case AttackType.Ranged:
                    switch (race)
                    {
                        case 0:
                            a = 28;
                            b = 0;
                            break;
                        case 1:
                            a = 44;
                            b = 10;
                            break;
                        case 2:
                            a = 34;
                            b = 0;
                            break;
                        case 3:
                            a = 34;
                            b = 0;
                            break;
                        case 4:
                            a = 44;
                            b = 2;
                            break;
                        case 5:
                            a = 28;
                            b = -2;
                            break;
                        case 6:
                            a = 34;
                            b = 0;
                            break;
                        case 8:
                            a = 15;
                            b = 0;
                            break;
                        default:
                            a = 0;
                            b = 0;
                            break;
                    }

                    return (int)Math.Floor(2 * calcLevel + 4 * weaponMod + a + Math.Floor((calcLevel - 1) * ((modifier + b) / 10.0)) * 2 + additional);
                case AttackType.Magical:
                    return 70 + additional;
                default:
                    return 0;
            }
        }

        public int GetDodge(int race, short armorLevel, short level, int modifier, int additional, bool isWild = true, short petLevel = 0, MateType mateType = MateType.Partner)
        {
            int calcLevel;
            int armorMod;
            int a;
            int b;

            if (isWild)
            {
                modifier = 0;
                calcLevel = level;
                armorMod = armorLevel;
            }
            else
            {
                calcLevel = petLevel;
                armorMod = petLevel + level - armorLevel;
            }

            if (!isWild && mateType == MateType.Partner)
            {
                return calcLevel + 9 + (int)Math.Floor((calcLevel - 1) * (modifier / 10.0));
            }

            switch (race)
            {
                case 0:
                    a = 26;
                    b = 0;
                    break;
                case 1:
                    a = 34;
                    b = 10;
                    break;
                case 2:
                    a = 29;
                    b = 0;
                    break;
                case 3:
                    a = 29;
                    b = 0;
                    break;
                case 4:
                    a = 34;
                    b = 2;
                    break;
                case 5:
                    a = 26;
                    b = -2;
                    break;
                case 6:
                    a = 29;
                    b = 0;
                    break;
                case 8:
                    a = 19;
                    b = 0;
                    break;
                default:
                    a = 0;
                    b = 0;
                    break;
            }

            return (int)Math.Floor(calcLevel + 4 * armorMod + a + Math.Floor((calcLevel - 1) * ((modifier + b) / 10.0)) + additional);
        }

        public int GetDefense(int race, AttackType attackType, short armorLevel, short level, int modifier, int additional, bool isWild = true, short petLevel = 0, MateType mateType = MateType.Pet)
        {
            int calcLevel;
            int armorMod;

            if (isWild)
            {
                modifier = 0;
                calcLevel = level;
                armorMod = armorLevel;
            }
            else
            {
                calcLevel = petLevel;
                armorMod = petLevel + level - armorLevel;
            }

            if (!isWild && mateType == MateType.Partner)
            {
                return (int)(0.5 * (calcLevel + 9 + (int)Math.Floor((calcLevel - 1) * (modifier / 10.0))));
            }

            double[] raceInfo = race switch
            {
                0 => DefenseRace0,
                1 => DefenseRace1,
                2 => DefenseRace2,
                3 => DefenseRace3,
                4 => DefenseRace4,
                5 => DefenseRace5,
                6 => DefenseRace6,
                8 => DefenseRace8,
                _ => DefaultDefense
            };

            return attackType switch
            {
                AttackType.Melee => (int)Math.Floor(2 * armorMod + raceInfo[0] + Math.Floor((armorMod + 5) * 0.08) + (calcLevel - 1) * ((modifier * 10 + (raceInfo[3] - 5 * modifier)) / 100.0) +
                    additional),
                AttackType.Ranged => (int)Math.Floor(2 * armorMod + raceInfo[1] + Math.Floor((armorMod + 5) * 0.36) + (calcLevel - 1) * ((modifier * 10 + (raceInfo[4] - 5 * modifier)) / 100.0) +
                    additional),
                AttackType.Magical => (int)Math.Floor(2 * armorMod + raceInfo[2] + Math.Floor((armorMod + 5) * 0.04) + (calcLevel - 1) * ((modifier * 10 + (raceInfo[5] - 5 * modifier)) / 100.0) +
                    additional)
            };
        }

        public byte GetSpeed(ClassType classType)
        {
            byte speed = classType switch
            {
                ClassType.Adventurer => 11,
                ClassType.Swordman => 11,
                ClassType.Archer => 12,
                ClassType.Magician => 10,
                ClassType.MartialArtist => 11,
                _ => 0
            };

            return speed;
        }
        
        public int GetTrainerSpecialistExperience(byte stars, byte heroLevel)
        {
            if (heroLevel == 0)
            {
                heroLevel = 1;
            }
            
            if (stars == 0)
            {
                stars = 1;
            }
            
            int maxExperienceByStars = stars switch
            {
                1 => 210,
                2 => 500,
                3 => 1080,
                4 => 2130,
                5 => 3500,
                6 => 5500,
                _ => 0,
            };

            int maxLevel = stars * 10;
            int experienceForLevel = maxExperienceByStars * heroLevel / maxLevel;
            return experienceForLevel;
        }

        public int GetStarsAttackPower(byte stars, byte heroLevel)
        {
            int baseAttackPower = 10;

            baseAttackPower += stars switch
            {
                2 => stars,
                3 => stars + 1,
                4 => stars + 2,
                5 => stars + 3,
                6 => stars + 4,
                _ => 0
            };

            return baseAttackPower * heroLevel;
        }

        public int GetStarsHitRate(byte stars, byte heroLevel)
        {
            int baseHitRate = 5;

            baseHitRate += stars switch
            {
                1 => stars,
                _ => stars - 1
            };

            return baseHitRate * heroLevel;
        }

        public int GetStarsAllDefences(byte stars, byte heroLevel)
        {
            int baseDefences = 4;

            baseDefences += stars switch
            {
                6 => stars,
                _ => stars - 1
            };

            return baseDefences * heroLevel;
        }

        public int GetStarsHpMp(byte stars, byte heroLevel)
        {
            int baseHpMp = 100;

            baseHpMp += stars switch
            {
                2 => stars * 10,
                3 => (stars + 1) * 10,
                4 => (stars + 2) * 10,
                5 => (stars + 3) * 10,
                6 => (stars + 9) * 10,
                _ => 0
            };

            return baseHpMp * heroLevel;
        }

        public int GetStarsAllResistances(byte stars, byte heroLevel)
        {
            int resistanceGrowth = 0;
            int startLevel = 1;
            int increment = (stars <= 2) ? 5 : (stars <= 4) ? 4 : (stars == 5) ? 3 : 2;

            for (int level = startLevel; level <= heroLevel; level++)
            {
                if ((level - startLevel + 1) % increment == 0)
                {
                    resistanceGrowth++;
                }
            }

            return resistanceGrowth;
        }

        private static double Mod(int a, int modulo)
        {
            if (a >= 0)
            {
                return a % modulo;
            }

            return Math.Abs((a - 2) % modulo);
        }
    }
}