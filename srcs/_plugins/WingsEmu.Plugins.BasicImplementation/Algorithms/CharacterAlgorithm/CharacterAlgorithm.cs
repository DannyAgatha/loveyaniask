using System.Collections.Generic;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.Game._enum;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.EntityStatistics;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace NosEmu.Plugins.BasicImplementations.Algorithms.CharacterAlgorithm;

public class CharacterAlgorithm : ICharacterAlgorithm
{
    public long GetLevelXp(short level, bool isMate = false, MateType mateType = 0)
    {
        long xp = level switch
        {
            1 => 300,
            2 => 840,
            3 => 1800,
            4 => 3300,
            5 => 5460,
            6 => 8400,
            7 => 12240,
            8 => 17100,
            9 => 23100,
            10 => 30360,
            11 => 39000,
            12 => 49140,
            13 => 60900,
            14 => 74400,
            15 => 105120,
            16 => 139800,
            17 => 178680,
            18 => 222000,
            19 => 270000,
            20 => 322920,
            21 => 381000,
            22 => 444480,
            23 => 513600,
            24 => 588600,
            25 => 669720,
            26 => 757200,
            27 => 851280,
            28 => 952200,
            29 => 1060200,
            30 => 1175520,
            31 => 1298400,
            32 => 1429080,
            33 => 1567800,
            34 => 1714800,
            35 => 1870320,
            36 => 2034600,
            37 => 2207880,
            38 => 2390400,
            39 => 2582400,
            40 => 3221180,
            41 => 3891500,
            42 => 4594120,
            43 => 5329800,
            44 => 6099300,
            45 => 6903380,
            46 => 7742800,
            47 => 8618320,
            48 => 9530700,
            49 => 10480700,
            50 => 11469080,
            51 => 12496600,
            52 => 13564020,
            53 => 14672100,
            54 => 15821600,
            55 => 17013280,
            56 => 18247900,
            57 => 19526220,
            58 => 20849000,
            59 => 22217000,
            60 => 27426400,
            61 => 32808000,
            62 => 38364600,
            63 => 44099000,
            64 => 50014000,
            65 => 56112400,
            66 => 62397000,
            67 => 68870600,
            68 => 75536000,
            69 => 82396000,
            70 => 89453400,
            71 => 96711000,
            72 => 104171600,
            73 => 111838000,
            74 => 119713000,
            75 => 127799400,
            76 => 136100000,
            77 => 144617600,
            78 => 153355000,
            79 => 162315000,
            80 => 195120000,
            81 => 228740000,
            82 => 263185000,
            83 => 326689000,
            84 => 391714000,
            85 => 487862000,
            86 => 586259000,
            87 => 755624100,
            88 => 947884800,
            89 => 1163765200,
            90 => 1403999800,
            91 => 1669333500,
            92 => 1960521600,
            93 => 2278329800,
            94 => 2623534200,
            95 => 3154654000,
            96 => 3739216800,
            97 => 4793630400,
            98 => 5972157000,
            99 => 7286681500,
            100 => 2214522000,
            101 => 2349774000,
            102 => 2487691000,
            103 => 2628299000,
            104 => 2771624000,
            105 => 2917692000,
            106 => 3066529000,
            107 => 3218161000,
            108 => 3372614000,
            109 => 3529914000,
            110 => 3690087000,
            111 => 3853159000,
            112 => 4019156000,
            113 => 4188104000,
            114 => 4360029000,
            115 => 4534957000,
            116 => 4712914000,
            117 => 4893926000,
            118 => 5078019000,
            119 => 5265219000,
            120 => 5455552000,
            121 => 5649044000,
            122 => 5845721000,
            123 => 6045609000,
            124 => 6248734000,
            125 => 6455122000,
            126 => 6664799000,
            127 => 6877791000,
            128 => 7094124000,
            129 => 7313824000,
            130 => 7536917000,
            131 => 7763429000,
            132 => 7993386000,
            133 => 8226814000,
            134 => 8463739000,
            135 => 8704187000,
            136 => 8948184000,
            137 => 9195756000,
            138 => 9446929000,
            139 => 9701729000,
            140 => 9960182000,
            141 => 10222314000,
            142 => 10488151000,
            143 => 10757719000,
            144 => 11031044000,
            145 => 11308152000,
            146 => 11589069000,
            147 => 11873821000,
            148 => 12162434000,
            149 => 12454934000,
            150 => 12751347000,
            _ => CalculateMaxXpWithMultipliers(level)
        };

        if (!isMate)
        {
            return xp;
        }

        return mateType == MateType.Partner ? (long)(xp * 0.2) : (long)(xp * 0.05);
    }

    public int GetHeroLevelXp(short level)
    {
        #region heroXpLabels

        int heroXp = level switch
        {
            0 => 1068540,
            1 => 1068540,
            2 => 1187520,
            3 => 1306500,
            4 => 1425480,
            5 => 1544460,
            6 => 1663440,
            7 => 1782420,
            8 => 1901400,
            9 => 2020380,
            10 => 2139360,
            11 => 2139360,
            12 => 2377320,
            13 => 2615280,
            14 => 2853240,
            15 => 3091200,
            16 => 3329160,
            17 => 3567120,
            18 => 3805080,
            19 => 4043040,
            20 => 4281000,
            21 => 4518960,
            22 => 5113860,
            23 => 5708760,
            24 => 6303660,
            25 => 6898560,
            26 => 7493460,
            27 => 8088360,
            28 => 8683260,
            29 => 9278160,
            30 => 9873060,
            31 => 10467960,
            32 => 11538780,
            33 => 12609600,
            34 => 13680420,
            35 => 14751240,
            36 => 15822060,
            37 => 16892880,
            38 => 17963700,
            39 => 19034520,
            40 => 20105340,
            41 => 21176160,
            42 => 23079840,
            43 => 24983520,
            44 => 26887200,
            45 => 28790880,
            46 => 30694560,
            47 => 32598240,
            48 => 34501920,
            49 => 36405600,
            50 => 38309280,
            51 => 40212960,
            52 => 42116640,
            53 => 44020320,
            54 => 45924000,
            55 => 47827680,
            56 => 49731360,
            57 => 51635040,
            58 => 53538720,
            59 => 55442400,
            60 => 56422420,
            61 => 59249760,
            62 => 61153440,
            63 => 63057120,
            64 => 64960800,
            65 => 66864480,
            66 => 68768160,
            67 => 70671840,
            68 => 72575520,
            69 => 74479200,
            70 => 76382880,
            71 => 78286560,
            72 => 80190240,
            73 => 82093920,
            74 => 83997600,
            75 => 85901280,
            76 => 87804960,
            77 => 89708640,
            78 => 91612320,
            79 => 93516000,
            80 => 95419680,
            _ => 0
        };

        #endregion

        return heroXp;
    }

    public int GetSpecialistJobXp(short level, bool isFunSpecialist = false)
    {
        #region spJobXpLabels

        if (isFunSpecialist)
        {
            return level switch
            {
                1 => 10000,
                2 => 10000,
                3 => 10000,
                4 => 10000,
                5 => 10000,
                6 => 10000,
                7 => 10000,
                8 => 10000,
                9 => 10000,
                10 => 10000,
                11 => 10000,
                12 => 10000,
                13 => 10000,
                14 => 10000,
                15 => 10000,
                16 => 10000,
                17 => 10000,
                18 => 10000,
                19 => 10000,
                20 => 100000,
                21 => 105000,
                22 => 110000,
                23 => 115000,
                24 => 120000,
                25 => 125000,
                26 => 130000,
                27 => 135000,
                28 => 140000,
                29 => 145000,
                30 => 150000,
                31 => 155000,
                32 => 160000,
                33 => 165000,
                34 => 170000,
                35 => 175000,
                36 => 180000,
                37 => 185000,
                38 => 304000,
                39 => 312000,
                40 => 320000,
                41 => 328000,
                42 => 336000,
                43 => 344000,
                44 => 352000,
                45 => 360000,
                46 => 368000,
                47 => 376000,
                48 => 672000,
                49 => 686000,
                50 => 700000,
                51 => 714000,
                52 => 728000,
                53 => 742000,
                54 => 756000,
                55 => 770000,
                56 => 784000,
                57 => 798000,
                58 => 812000,
                59 => 826000,
                60 => 840000,
                61 => 854000,
                62 => 868000,
                63 => 882000,
                64 => 896000,
                65 => 910000,
                66 => 924000,
                67 => 938000,
                68 => 952000,
                69 => 966000,
                70 => 980000,
                71 => 994000,
                72 => 1008000,
                73 => 1022000,
                74 => 1036000,
                75 => 1050000,
                76 => 1064000,
                77 => 1078000,
                78 => 1092000,
                79 => 1106000,
                80 => 1120000,
                81 => 1134000,
                82 => 1148000,
                83 => 1162000,
                84 => 1176000,
                85 => 1190000,
                86 => 1204000,
                87 => 1218000,
                88 => 1232000,
                89 => 1246000,
                90 => 1260000,
                91 => 1274000,
                92 => 1288000,
                93 => 1302000,
                94 => 1316000,
                95 => 1330000,
                96 => 1344000,
                97 => 1358000,
                98 => 1372000,
                99 => 1386000,
                _ => 1400000
            };
        }

        int spJobXp = level switch
        {
            1 => 15000,
            2 => 25000,
            3 => 35000,
            4 => 45000,
            5 => 55000,
            6 => 65000,
            7 => 75000,
            8 => 85000,
            9 => 95000,
            10 => 105000,
            11 => 115000,
            12 => 125000,
            13 => 135000,
            14 => 145000,
            15 => 155000,
            16 => 165000,
            17 => 175000,
            18 => 185000,
            19 => 195000,
            20 => 23000,
            21 => 38132,
            22 => 53264,
            23 => 68396,
            24 => 83528,
            25 => 98660,
            26 => 113792,
            27 => 128924,
            28 => 144056,
            29 => 159188,
            30 => 174320,
            31 => 189452,
            32 => 204584,
            33 => 219716,
            34 => 234848,
            35 => 249980,
            36 => 265112,
            37 => 280244,
            38 => 295376,
            39 => 310508,
            40 => 325640,
            41 => 363470,
            42 => 401300,
            43 => 439130,
            44 => 476960,
            45 => 514790,
            46 => 552620,
            47 => 590450,
            48 => 628280,
            49 => 666110,
            50 => 703940,
            51 => 741770,
            52 => 779600,
            53 => 817430,
            54 => 855260,
            55 => 893090,
            56 => 930920,
            57 => 968750,
            58 => 1006580,
            59 => 1044410,
            60 => 1082240,
            61 => 1150334,
            62 => 1218428,
            63 => 1286522,
            64 => 1354616,
            65 => 1422710,
            66 => 1490804,
            67 => 1558898,
            68 => 1626992,
            69 => 1695086,
            70 => 1763180,
            71 => 1831274,
            72 => 1899368,
            73 => 1967462,
            74 => 2035556,
            75 => 2103650,
            76 => 2171744,
            77 => 2239838,
            78 => 2307932,
            79 => 2376026,
            80 => 2444120,
            81 => 2572742,
            82 => 2701364,
            83 => 2829986,
            84 => 2958608,
            85 => 3087230,
            86 => 3215852,
            87 => 3344474,
            88 => 3473096,
            89 => 3601718,
            90 => 3730340,
            91 => 3858962,
            92 => 3987584,
            93 => 4116206,
            94 => 4244828,
            95 => 4373450,
            96 => 4502072,
            97 => 4630694,
            98 => 4759316,
            99 => 4887938,
            _ => 0
        };

        #endregion

        return spJobXp;
    }

    public int GetJobXp(short level, bool isAdventurer = false)
    {
        #region adventurerJobXpLabels

        if (isAdventurer)
        {
            int adventurerJobXp = level switch
            {
                1 => 2200,
                2 => 2900,
                3 => 3600,
                4 => 4300,
                5 => 5000,
                6 => 5700,
                7 => 6400,
                8 => 7100,
                9 => 7800,
                10 => 8500,
                11 => 9200,
                12 => 9900,
                13 => 10600,
                14 => 11300,
                15 => 12000,
                16 => 12700,
                17 => 13400,
                18 => 14100,
                19 => 14800,
                20 => 15500
            };

            return adventurerJobXp;
        }

        #endregion

        #region jobXpLabels

        int jobXp = level switch
        {
            1 => 14500,
            2 => 19000,
            3 => 23500,
            4 => 28000,
            5 => 32500,
            6 => 37000,
            7 => 41500,
            8 => 46000,
            9 => 50500,
            10 => 55000,
            11 => 59500,
            12 => 64000,
            13 => 68500,
            14 => 73000,
            15 => 77500,
            16 => 82000,
            17 => 86500,
            18 => 91000,
            19 => 95500,
            20 => 100000,
            21 => 104500,
            22 => 109000,
            23 => 113500,
            24 => 118000,
            25 => 122500,
            26 => 127000,
            27 => 131500,
            28 => 136000,
            29 => 140500,
            30 => 145000,
            31 => 149500,
            32 => 154000,
            33 => 158500,
            34 => 163000,
            35 => 167500,
            36 => 172000,
            37 => 176500,
            38 => 181000,
            39 => 185500,
            40 => 190000,
            41 => 205000,
            42 => 220000,
            43 => 235000,
            44 => 250000,
            45 => 265000,
            46 => 280000,
            47 => 295000,
            48 => 310000,
            49 => 325000,
            50 => 340000,
            51 => 355000,
            52 => 370000,
            53 => 385000,
            54 => 400000,
            55 => 415000,
            56 => 430000,
            57 => 445000,
            58 => 460000,
            59 => 475000,
            60 => 490000,
            61 => 505000,
            62 => 520000,
            63 => 535000,
            64 => 550000,
            65 => 565000,
            66 => 580000,
            67 => 595000,
            68 => 610000,
            69 => 625000,
            70 => 640000,
            71 => 655000,
            72 => 670000,
            73 => 685000,
            74 => 700000,
            75 => 715000,
            76 => 730000,
            77 => 745000,
            78 => 760000,
            79 => 775000,
            80 => 790000,
            81 => 805000,
            82 => 820000,
            83 => 835000,
            84 => 850000,
            85 => 865000,
            86 => 880000,
            87 => 895000,
            88 => 910000,
            89 => 925000,
            90 => 940000,
            91 => 955000,
            92 => 970000,
            93 => 985000,
            94 => 1000000,
            95 => 1015000,
            96 => 1030000,
            97 => 1045000,
            98 => 1060000,
            99 => 1075000,
            100 => 1090000
        };

        #endregion

        return jobXp;
    }

    public int GetFairyXp(short level)
    {
        if (level < 40)
        {
            return level * level + 50;
        }

        return level * level * 3 + 50;
    }

    public int GetRegenHp(IPlayerEntity character, ClassType type, bool isSiting)
    {
        if (character.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
        {
            return 0;
        }
        
        int regenHp = type switch
        {
            ClassType.Adventurer => 30,
            ClassType.Swordman => 80,
            ClassType.Archer => 60,
            ClassType.Magician => 30,
            ClassType.MartialArtist => 70,
            _ => 0
        };

        regenHp += character.StatisticsComponent.Passives.GetValueOrDefault(PassiveType.REGEN_HP);
        regenHp += character.GetJewelsCellonsValue(CellonType.HpRecovery);
        regenHp = (int)(regenHp * (1 + character.GetMaxArmorShellValue(ShellEffectType.RecovryHPOnRest) * 0.01));
        (int firstData, int secondData) hpBCards = character.BCardComponent.GetAllBCardsInformation(BCardType.Recovery, (byte)AdditionalTypes.Recovery.HPRecoveryIncreased, character.Level);
        regenHp += hpBCards.firstData;

        if (isSiting)
        {
            return regenHp;
        }

        double levelPenalty = character.Level switch
        {
            < 21 => 0.5,
            < 41 => 0.4,
            < 61 => 0.3,
            _ => 0.2
        };

        int regen = character.StatisticsComponent.Passives.GetValueOrDefault(PassiveType.PASSIVE_REGEN);
        regenHp = (int)(regenHp * levelPenalty * regen + regenHp * character.GetMaxArmorShellValue(ShellEffectType.RevoryHP) * 0.01);

        return regenHp;
    }

    public int GetRegenMp(IPlayerEntity character, ClassType type, bool isSiting)
    {
        if (character.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
        {
            return 0;
        }
        
        int regenMp = type switch
        {
            ClassType.Adventurer => 10,
            ClassType.Swordman => 30,
            ClassType.Archer => 50,
            ClassType.Magician => 80,
            ClassType.MartialArtist => 40,
            _ => 0
        };

        regenMp += character.StatisticsComponent.Passives.GetValueOrDefault(PassiveType.REGEN_MP);
        regenMp += character.GetJewelsCellonsValue(CellonType.MpRecovery);
        regenMp = (int)(regenMp * (1 + character.GetMaxArmorShellValue(ShellEffectType.RecoveryMPOnRest) * 0.01));
        (int firstData, int secondData) hpBCards = character.BCardComponent.GetAllBCardsInformation(BCardType.Recovery, (byte)AdditionalTypes.Recovery.MPRecoveryIncreased, character.Level);
        regenMp += hpBCards.firstData;

        if (isSiting)
        {
            return regenMp;
        }

        double levelPenalty = character.Level switch
        {
            < 21 => 0.5,
            < 41 => 0.4,
            < 61 => 0.3,
            _ => 0.2
        };

        int regen = character.StatisticsComponent.Passives.GetValueOrDefault(PassiveType.PASSIVE_REGEN);
        regenMp = (int)(regenMp * levelPenalty * regen + regenMp * character.GetMaxArmorShellValue(ShellEffectType.RecoveryMP) * 0.01);

        return regenMp;
    }

    public long CalculateMaxXp(short level)
    {
        if (level <= 1)
        {
            return 300;
        }

        long xp = 120 * (1 + level) * (1 + level) / 2;

        if (level >= 15)
        {
            xp *= 2;
        }

        if (level >= 85)
        {
            xp *= 650;
            xp /= 6;
        }
        else if (level >= 83)
        {
            xp *= 450;
            xp /= 6;
        }
        else if (level >= 80)
        {
            xp *= 250;
            xp /= 6;
        }
        else if (level >= 60)
        {
            xp *= 70;
            xp /= 6;
        }
        else if (level >= 40)
        {
            xp *= 19;
            xp /= 6;
        }

        return xp + CalculateMaxXp((short)(level - 1));
    }

    private long CalculateMaxXpWithMultipliers(short level)
    {
        double multiplier = level switch
        {
            87 => 1.1,
            88 => 1.2,
            89 => 1.3,
            90 => 1.4,
            91 => 1.5,
            92 => 1.6,
            93 => 1.7,
            94 => 1.8,
            95 => 2.0,
            96 => 2.2,
            97 => 2.6,
            98 => 3.0,
            99 => 3.5,
            > 99 => 3.5,
            _ => 1
        };

        return (long)(multiplier * CalculateMaxXp(level));
    }
}