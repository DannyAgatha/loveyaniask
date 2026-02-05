using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PhoenixLib.Logging;
using WingsAPI.Data.GameData;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game.Buffs;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace Plugin.ResourceLoader.Loaders
{
    public class SkillResourceFileLoader : IResourceLoader<SkillDTO>
    {
        private readonly ResourceLoadingConfiguration _config;
        private readonly ILogger<SkillResourceFileLoader> _logger;
        private readonly List<SkillDTO> _skills = new();

        public SkillResourceFileLoader(ResourceLoadingConfiguration config, ILogger<SkillResourceFileLoader> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<IReadOnlyList<SkillDTO>> LoadAsync()
        {
            if (_skills.Any())
            {
                return _skills;
            }

            string filePath = Path.Combine(_config.GameDataPath, "Skill.dat");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} should be present");
            }


            var skill = new SkillDTO();

            int counter = 0;
            using var skillIdStream = new StreamReader(filePath, Encoding.GetEncoding(1252));
            string line;
            while ((line = await skillIdStream.ReadLineAsync()) != null)
            {
                string[] currentLine = line.Split('\t');

                switch (currentLine.Length)
                {
                    case > 2 when currentLine[1] == "VNUM":
                        skill = new SkillDTO
                        {
                            Id = short.Parse(currentLine[2])
                        };
                        break;
                    case > 2 when currentLine[1] == "NAME":
                        skill.Name = currentLine[2];
                        break;
                    case > 2 when currentLine[1] == "TYPE":
                        skill.SkillType = (SkillType)byte.Parse(currentLine[2]);
                        skill.CastId = short.Parse(currentLine[3]);
                        skill.Class = byte.Parse(currentLine[4]);
                        skill.AttackType = (AttackType)byte.Parse(currentLine[5]);
                        skill.IsUsingSecondWeapon = currentLine[6] == "1";
                        skill.Element = byte.Parse(currentLine[7]);
                        break;
                    case > 3 when currentLine[1] == "COST":
                        skill.CPCost = currentLine[2] == "-1" ? (byte)0 : byte.Parse(currentLine[2]);
                        skill.Price = int.Parse(currentLine[3]);
                        skill.SpecialCost = int.Parse(currentLine[4]);
                        break;
                    case > 2 when currentLine[1] == "LEVEL":
                        FillLevelInformation(skill, currentLine, _skills);
                        break;
                    case > 2 when currentLine[1] == "EFFECT":
                        skill.CtEffect = short.Parse(currentLine[3]);
                        skill.CtAnimation = short.Parse(currentLine[4]);
                        skill.SuEffect = short.Parse(currentLine[5]);
                        skill.SuAnimation = short.Parse(currentLine[6]);
                        break;
                    case > 2 when currentLine[1] == "TARGET":
                        skill.TargetType = (TargetType)byte.Parse(currentLine[2]);
                        skill.HitType = (TargetHitType)byte.Parse(currentLine[3]);
                        skill.Range = byte.Parse(currentLine[4]);
                        skill.AoERange = short.Parse(currentLine[5]);
                        skill.TargetAffectedEntities = (TargetAffectedEntities)byte.Parse(currentLine[6]);
                        break;
                    case > 2 when currentLine[1] == "DATA":
                        skill.UpgradeSkill = short.Parse(currentLine[2]);
                        skill.UpgradeType = short.Parse(currentLine[3]);
                        skill.CastTime = short.Parse(currentLine[6]);
                        skill.Cooldown = short.Parse(currentLine[7]);
                        skill.MpCost = short.Parse(currentLine[10]);
                        skill.DashSpeed = short.Parse(currentLine[11]);
                        skill.ItemVNum = short.Parse(currentLine[12]);
                        // TODO: Temporal fix, need to clean. - Dazynnn
                        switch (skill.Id)
                        {
                            case 1100:
                                skill.Cooldown = 10;
                                break;
                        }
                        break;
                    case > 2 when currentLine[1] == "BASIC":
                    {
                        byte type = (byte)int.Parse(currentLine[3]);
                        if (type is 0 or 255)
                        {
                            continue;
                        }

                        int first = int.Parse(currentLine[5]);
                        int second = int.Parse(currentLine[6]);

                        int firstModulo = first % 4;
                        firstModulo = firstModulo switch
                        {
                            -1 => 1,
                            -2 => 2,
                            -3 => 1,
                            _ => firstModulo
                        };

                        int secondModulo = second % 4;
                        secondModulo = secondModulo switch
                        {
                            -1 => 1,
                            -2 => 2,
                            -3 => 1,
                            _ => secondModulo
                        };

                        var bcard = new BCardDTO
                        {
                            SkillVNum = skill.Id,
                            Type = type,
                            SubType = (byte)((int.Parse(currentLine[4]) + 1) * 10 + 1 + (first < 0 ? 1 : 0)),
                            FirstDataScalingType = (BCardScalingType)firstModulo,
                            SecondDataScalingType = (BCardScalingType)secondModulo,
                            FirstData = (int)Math.Abs(Math.Floor(first / 4.0)),
                            SecondData = (int)Math.Abs(Math.Floor(second / 4.0)),
                            CastType = byte.Parse(currentLine[7])
                        };

                        skill.BCards.Add(bcard);
                        AddCustomBcardMissing(skill);
                        break;
                    }

                    case > 2 when currentLine[1] == "FCOMBO":
                    {
                        if (int.Parse(currentLine[2]) == 0)
                        {
                            continue;
                        }

                        for (int i = 3; i < currentLine.Length - 4; i += 3)
                        {
                            var comb = new ComboDTO
                            {
                                SkillVNum = skill.Id,
                                Hit = short.Parse(currentLine[i]),
                                Animation = short.Parse(currentLine[i + 1]),
                                Effect = short.Parse(currentLine[i + 2])
                            };

                            if (comb.Hit == 0 && comb.Animation == 0 && comb.Effect == 0)
                            {
                                continue;
                            }

                            /*
                             * Idk if it's preferable to load them all instead of one by one (idk if there is lot of combo skills)
                             */
                            skill.Combos.Add(comb);
                        }

                        break;
                    }
                    case > 2 when currentLine[1] == "CELL":
                        // investigate
                        break;
                    case > 1 when currentLine[1] == "Z_DESC":
                        _skills.Add(skill);
                        counter++;
                        break;
                }
            }

            void AddCustomBcardMissing(SkillDTO skill)
            {
                if (skill.Id != 1316) return;
                var bcard = new BCardDTO
                {
                    SkillVNum = skill.Id,
                    Type = (short)BCardType.Summons,
                    SubType = (byte)AdditionalTypes.Summons.Summons,
                    FirstDataScalingType = BCardScalingType.NORMAL_VALUE,
                    SecondDataScalingType = BCardScalingType.NORMAL_VALUE,
                    FirstData = 2,
                    SecondData = 2341,
                    CastType = (byte)CastType.ATTACK
                };
                var bcard2 = new BCardDTO
                {
                    SkillVNum = skill.Id,
                    Type = (short)BCardType.Summons,
                    SubType = (byte)AdditionalTypes.Summons.Summons,
                    FirstDataScalingType = BCardScalingType.NORMAL_VALUE,
                    SecondDataScalingType = BCardScalingType.NORMAL_VALUE,
                    FirstData = 2,
                    SecondData = 2342,
                    CastType = (byte)CastType.ATTACK
                };

                skill.BCards.Add(bcard);
                skill.BCards.Add(bcard2);
            }

            Log.Info($"[RESOURCE_LOADER] {counter.ToString()} Skills loaded");
            return _skills;
        }

        private static void FillLevelInformation(SkillDTO skill, IReadOnlyList<string> currentLine, IReadOnlyCollection<SkillDTO> skills)
        {
            skill.LevelMinimum = currentLine[2] != "-1" ? byte.Parse(currentLine[2]) : (byte)0;
            if (skill.Class > 31)
            {
                SkillDTO firstSkill = skills.FirstOrDefault(s => s.Class == skill.Class);
                if (firstSkill == null || skill.Id <= firstSkill.Id + 10)
                {
                    skill.LevelMinimum = skill.Class switch
                    {
                        8 => skills.Count(s => s.Class == skill.Class) switch
                        {
                            3 => 20,
                            2 => 10,
                            _ => 0,
                        },
                        9 => skills.Count(s => s.Class == skill.Class) switch
                        {
                            9 => 20,
                            8 => 16,
                            7 => 12,
                            6 => 8,
                            5 => 4,
                            _ => 0,
                        },
                        16 => skills.Count(s => s.Class == skill.Class) switch
                        {
                            6 => 20,
                            5 => 15,
                            4 => 10,
                            3 => 5,
                            2 => 3,
                            _ => 0,
                        },
                        _ => skills.Count(s => s.Class == skill.Class) switch
                        {
                            10 => 20,
                            9 => 16,
                            8 => 12,
                            7 => 8,
                            6 => 4,
                            _ => 0,
                        },
                    };
                }
            }

            switch (skill.Id)
            {
                case 794:
                case 797:
                case 798:
                    skill.LevelMinimum = 0;
                    break;
                case 967:
                    skill.LevelMinimum = 3;
                    break;
                case 968:
                    skill.LevelMinimum = 5;
                    break;
                case 970:
                case 1014:
                case 1015:
                case 1790:
                    skill.LevelMinimum = 10;
                    break;
                case 971:
                case 1016:
                    skill.LevelMinimum = 15;
                    break;
                case 1006:
                case 1018:
                case 1791:
                    skill.LevelMinimum = 20;
                    break;
                case 1007:
                case 1008:
                case 1017:
                case 1792:
                    skill.LevelMinimum = 25;
                    break;
                case 1020:
                case 1793:
                    skill.LevelMinimum = 30;
                    break;
                case 1019:
                case 1794:
                    skill.LevelMinimum = 40;
                    break;
                case 1009:
                case 1795:
                    skill.LevelMinimum = 45;
                    break;
                case 1796:
                    skill.LevelMinimum = 50;
                    break;
            }
            
            skill.SuAnimation = skill.Id switch
            {
                1393 => 11,
                _ => skill.SuAnimation
            };
            
            skill.MinimumAdventurerLevel = currentLine[3] != "-1" ? byte.Parse(currentLine[3]) : (byte)0;
            skill.MinimumSwordmanLevel = currentLine[4] != "-1" ? byte.Parse(currentLine[4]) : (byte)0;
            skill.MinimumArcherLevel = currentLine[5] != "-1" ? byte.Parse(currentLine[5]) : (byte)0;
            skill.MinimumMagicianLevel = currentLine[6] != "-1" ? byte.Parse(currentLine[6]) : (byte)0;
        }
    }
}