using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PhoenixLib.Logging;
using WingsAPI.Data.Drops;
using WingsAPI.Data.GameData;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.NpcMonster;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game.Algorithm;
using WingsEmu.Packets.Enums.Battle;

namespace Plugin.ResourceLoader.Loaders
{
    public class NpcMonsterFileLoader : IResourceLoader<NpcMonsterDto>
    {
        private readonly IBattleEntityAlgorithmService _algorithm;
        private readonly ResourceLoadingConfiguration _config;
        private readonly ILogger<NpcMonsterFileLoader> _logger;
        private readonly List<NpcMonsterDto> _npcMonsters = new();

        public NpcMonsterFileLoader(ResourceLoadingConfiguration config, ILogger<NpcMonsterFileLoader> logger, IBattleEntityAlgorithmService algorithm)
        {
            _config = config;
            _logger = logger;
            _algorithm = algorithm;
        }

        public async Task<IReadOnlyList<NpcMonsterDto>> LoadAsync()
        {
            if (_npcMonsters.Any())
            {
                return _npcMonsters;
            }

            string filePath = Path.Combine(_config.GameDataPath, "monster.dat");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} should be present");
            }

            var npc = new NpcMonsterDto();

            bool itemAreaBegin = false;
            int counter = 0;

            using var npcIdStream = new StreamReader(filePath, Encoding.GetEncoding(1252));
            string line;
            while ((line = await npcIdStream.ReadLineAsync()) != null)
            {
                string[] currentLine = line.Split('\t');

                switch (currentLine.Length)
                {
                    case > 2 when currentLine[1] == "VNUM":
                        npc = new NpcMonsterDto
                        {
                            Id = Convert.ToInt16(currentLine[2])
                        };
                        itemAreaBegin = true;
                        break;
                    case > 2 when currentLine[1] == "NAME":
                        npc.Name = currentLine[2];
                        break;
                    case > 2 when currentLine[1] == "LEVEL":
                        {
                            if (!itemAreaBegin)
                            {
                                continue;
                            }

                            npc.Level = Convert.ToByte(currentLine[2]);
                            break;
                        }
                    case > 3 when currentLine[1] == "RACE":
                        npc.Race = Convert.ToByte(currentLine[2]);
                        npc.RaceType = Convert.ToByte(currentLine[3]);
                        npc.HeroLevel = Convert.ToByte(currentLine[4]);
                        break;
                    case > 7 when currentLine[1] == "ATTRIB":
                        npc.Element = Convert.ToByte(currentLine[2]);
                        npc.ElementRate = Convert.ToInt16(currentLine[3]);
                        npc.FireResistance = Convert.ToInt16(currentLine[4]);
                        npc.WaterResistance = Convert.ToInt16(currentLine[5]);
                        npc.LightResistance = Convert.ToInt16(currentLine[6]);
                        npc.DarkResistance = Convert.ToInt16(currentLine[7]);
                        break;
                    case > 2 when currentLine[1] == "EXP":
                        {
                            npc.BaseXp = Convert.ToInt32(currentLine[2]);
                            npc.BaseJobXp = Convert.ToInt32(currentLine[3]);

                            npc.Xp = npc.Level < 20 ? 60 * npc.Level + Convert.ToInt32(currentLine[2]) : 70 * npc.Level + Convert.ToInt32(currentLine[2]);
                            npc.JobXp = npc.Level > 60 ? 105 + Convert.ToInt32(currentLine[3]) : 120 + Convert.ToInt32(currentLine[3]);

                            if (npc.Xp < 0)
                            {
                                npc.Xp = 0;
                            }

                            if (npc.JobXp < 0)
                            {
                                npc.JobXp = 0;
                            }

                            break;
                        }
                    case > 6 when currentLine[1] == "PREATT":
                        npc.HostilityType = Convert.ToInt32(currentLine[2]);
                        npc.GroupAttack = Convert.ToInt32(currentLine[3]);
                        npc.NoticeRange = Convert.ToInt16(currentLine[4]);
                        npc.Speed = Convert.ToByte(currentLine[5]);
                        npc.RespawnTime = Convert.ToInt32(currentLine[6]);
                        break;
                    case > 4 when currentLine[1] == "WINFO":
                        npc.WinfoValue = Convert.ToByte(currentLine[3]);
                        npc.AttackUpgrade = Convert.ToByte(currentLine[4]);
                        break;
                    case > 3 when currentLine[1] == "AINFO":
                        npc.DefenceUpgrade = Convert.ToByte(currentLine[3]);
                        break;
                    case > 6 when currentLine[1] == "PETINFO":
                        {
                            if (npc.Race == 8)
                            {
                                switch (npc.RaceType)
                                {
                                    case 7: //collectable NPC
                                        npc.MaxTries = Convert.ToByte(currentLine[2]);
                                        npc.CollectionCooldown = Convert.ToInt16(currentLine[3]);
                                        npc.AmountRequired = Convert.ToInt16(currentLine[4]);
                                        npc.CollectionDanceTime = Convert.ToByte(currentLine[5]);
                                        break;
                                    case 5: //teleporters
                                        npc.VNumRequired = Convert.ToInt16(currentLine[2]);
                                        npc.AmountRequired = Convert.ToInt16(currentLine[3]);
                                        npc.TeleportRemoveFromInventory = currentLine[4] != "0";
                                        break;
                                }
                            }
                            else
                            {
                                npc.MeleeHpFactor = Convert.ToInt16(currentLine[2]);
                                npc.RangeDodgeFactor = Convert.ToInt16(currentLine[3]);
                                npc.MagicMpFactor = Convert.ToInt16(currentLine[4]);
                                npc.Stars = Convert.ToByte(currentLine[6]);
                            }
                            break;
                        }
                    case > 3 when currentLine[1] == "HP/MP":
                        npc.CleanHp = Convert.ToInt32(currentLine[2]);
                        npc.CleanMp = Convert.ToInt32(currentLine[3]);

                        npc.MaxHp = _algorithm.GetBasicHp(npc.Race, npc.Level, npc.MeleeHpFactor, Convert.ToInt32(currentLine[2]));
                        npc.MaxMp = _algorithm.GetBasicMp(npc.Race, npc.Level, npc.MagicMpFactor, Convert.ToInt32(currentLine[3]));
                        
                        switch (npc.Id)
                        {
                            case 1496:
                            case 1497:
                            case 1498:
                                npc.MaxMp = 900;
                                break;
                            case 1532:
                            case 1533:
                            case 1534:
                                npc.MaxMp = 1800;
                                break;
                            case 1535:
                            case 1536:
                            case 1537:
                                npc.MaxMp = 3600;
                                break;
                        }
                        break;
                    case > 6 when currentLine[1] == "WEAPON":
                        npc.WeaponLevel = Convert.ToByte(currentLine[2]);
                        npc.CleanDamageMin = Convert.ToInt32(currentLine[4]);
                        npc.CleanDamageMax = Convert.ToInt32(currentLine[5]);
                        npc.CleanHitRate = Convert.ToInt32(currentLine[6]);

                        npc.DamageMinimum = _algorithm.GetAttack(true, npc.Race, npc.AttackType, npc.WeaponLevel, npc.WinfoValue, npc.Level, GetModifier(npc),
                            Convert.ToInt16(currentLine[4]));
                        npc.DamageMaximum = _algorithm.GetAttack(false, npc.Race, npc.AttackType, npc.WeaponLevel, npc.WinfoValue, npc.Level, GetModifier(npc),
                            Convert.ToInt16(currentLine[5]));
                        npc.Concentrate = (short)_algorithm.GetHitrate(npc.Race, npc.AttackType, npc.WeaponLevel, npc.Level, GetModifier(npc),
                            Convert.ToInt16(currentLine[6]));
                        npc.CriticalChance = (short)(Convert.ToInt16(currentLine[7]) + 4);
                        npc.CriticalRate = (short)(Convert.ToInt16(currentLine[8]) + 70);
                        break;
                    case > 6 when currentLine[1] == "ARMOR":
                        npc.ArmorLevel = Convert.ToByte(currentLine[2]);
                        npc.CleanMeleeDefence = Convert.ToInt32(currentLine[3]);
                        npc.CleanRangeDefence = Convert.ToInt32(currentLine[4]);
                        npc.CleanMagicDefence = Convert.ToInt32(currentLine[5]);
                        npc.CleanDodge = Convert.ToInt32(currentLine[6]);

                        npc.CloseDefence = (short)_algorithm.GetDefense(npc.Race, AttackType.Melee, npc.ArmorLevel, npc.Level, GetModifier(npc), Convert.ToInt16(currentLine[3]));
                        npc.DistanceDefence = (short)_algorithm.GetDefense(npc.Race, AttackType.Ranged, npc.ArmorLevel, npc.Level, GetModifier(npc), Convert.ToInt16(currentLine[4]));
                        npc.MagicDefence = (short)_algorithm.GetDefense(npc.Race, AttackType.Magical, npc.ArmorLevel, npc.Level, GetModifier(npc), Convert.ToInt16(currentLine[5]));
                        npc.DefenceDodge = (short)_algorithm.GetDodge(npc.Race, npc.ArmorLevel, npc.Level, GetModifier(npc), Convert.ToInt16(currentLine[6]));
                        npc.DistanceDefenceDodge = (short)_algorithm.GetDodge(npc.Race, npc.ArmorLevel, npc.Level, GetModifier(npc), Convert.ToInt16(currentLine[6]));
                        break;
                    case > 7 when currentLine[1] == "ETC":
                        {
                            long bitFlag = Convert.ToInt64(currentLine[2]);

                            npc.CanWalk = Convert.ToBoolean(bitFlag & (long)MobFlag.CANT_WALK) == false;
                            npc.CanBeCollected = Convert.ToBoolean(bitFlag & (long)MobFlag.CAN_BE_COLLECTED);
                            npc.CanBeDebuffed = Convert.ToBoolean(bitFlag & (long)MobFlag.CANT_BE_DEBUFFED) == false;
                            npc.CanBeCaught = Convert.ToBoolean(bitFlag & (long)MobFlag.CAN_BE_CAUGHT);
                            npc.DisappearAfterSeconds = Convert.ToBoolean(bitFlag & (long)MobFlag.DISSAPPEAR_AFTER_SECONDS);
                            if (!npc.DisappearAfterSeconds)
                            {
                                npc.DisappearAfterSeconds = Convert.ToBoolean(bitFlag & (long)MobFlag.DISSAPPEAR_AFTER_SECONDS_2);
                            }
                            npc.DisappearAfterHitting = Convert.ToBoolean(bitFlag & (long)MobFlag.DISSAPPEAR_AFTER_HITTING);
                            npc.HasMode = Convert.ToBoolean(bitFlag & (long)MobFlag.HAS_MODE);
                            npc.DisappearAfterSecondsMana = Convert.ToBoolean(bitFlag & (long)MobFlag.DISSAPPEAR_AFTER_SECONDS_MANA);
                            npc.OnDefenseOnlyOnce = Convert.ToBoolean(bitFlag & (long)MobFlag.ON_DEFENSE_ONLY_ONCE);
                            npc.HasDash = Convert.ToBoolean(bitFlag & (long)MobFlag.HAS_DASH);
                            npc.CanRegenMp = Convert.ToBoolean(bitFlag & (long)MobFlag.CAN_REGEN_MP);
                            npc.CanBePushed = Convert.ToBoolean(bitFlag & (long)MobFlag.CAN_BE_PUSHED) == false;

                            npc.IsPercent = currentLine[4] == "1";
                            npc.DamagedOnlyLastJajamaruSkill = currentLine[5] == "1";
                            npc.DropToInventory = currentLine[7] == "1";
                            break;
                        }
                    case > 6 when currentLine[1] == "SETTING":
                        {
                            npc.IconId = Convert.ToInt32(currentLine[2]);
                            npc.SpawnMobOrColor = Convert.ToInt32(currentLine[3]);
                            npc.SpriteSize = Convert.ToInt32(currentLine[5]);
                            npc.CellSize = Convert.ToInt32(currentLine[6]);

                            if (npc.Race == 8 && (npc.RaceType == 7 || npc.RaceType == 6))
                            {
                                npc.VNumRequired = Convert.ToInt16(currentLine[4]);
                            }

                            break;
                        }
                    case > 2 when currentLine[1] == "EFF":
                        npc.AttackEffect = Convert.ToInt16(currentLine[2]);
                        npc.PermanentEffect = Convert.ToInt16(currentLine[3]);
                        npc.DeathEffect = Convert.ToInt16(currentLine[4]);
                        break;
                    case > 8 when currentLine[1] == "ZSKILL":
                        npc.AttackType = (AttackType)Convert.ToByte(currentLine[2]);
                        npc.BasicRange = Convert.ToByte(currentLine[3]);
                        npc.BasicHitChance = Convert.ToByte(currentLine[4]);
                        npc.BasicCastTime = Convert.ToByte(currentLine[5]);
                        npc.BasicCooldown = Convert.ToInt16(currentLine[6]);
                        npc.BasicDashSpeed = Convert.ToInt16(currentLine[7]);
                        break;
                    case > 1 when currentLine[1] == "SKILL":
                        {
                            for (int i = 2; i < currentLine.Length; i += 3)
                            {
                                short vnum = short.Parse(currentLine[i]);
                                
                                if (vnum is -1 or 0)
                                {
                                    continue;
                                }

                                npc.Skills.Add(new NpcMonsterSkillDTO
                                {
                                    SkillVNum = vnum,
                                    Rate = Convert.ToInt16(currentLine[i + 1]),
                                    NpcMonsterVNum = npc.Id,
                                    IsBasicAttack = currentLine[i + 2] == "2",
                                    IsIgnoringHitChance = currentLine[i + 2] == "1"
                                });
                            }

                            break;
                        }
                    case > 1 when currentLine[1] == "MODE":
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                byte type = (byte)int.Parse(currentLine[5 * i + 2]);
                                if (type == 0)
                                {
                                    continue;
                                }

                                int first = int.Parse(currentLine[3 + 5 * i]);
                                int second = int.Parse(currentLine[4 + 5 * i]);

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

                                var modeBCard = new BCardDTO
                                {
                                    NpcMonsterVNum = npc.Id,
                                    Type = type,
                                    SubType = (byte)((int.Parse(currentLine[5 + 5 * i]) + 1) * 10 + 1 + (first >= 0 ? 0 : 1)),
                                    FirstDataScalingType = (BCardScalingType)firstModulo,
                                    SecondDataScalingType = (BCardScalingType)secondModulo,
                                    FirstData = (int)Math.Abs(Math.Floor(first / 4.0)),
                                    SecondData = (int)Math.Abs(Math.Floor(second / 4.0)),
                                    CastType = byte.Parse(currentLine[6 + 5 * i]),
                                    IsMonsterMode = true
                                };

                                npc.ModeBCards.Add(modeBCard);
                            }

                            npc.ModeIsHpTriggered = currentLine[27] == "0";
                            npc.ModeLimiterType = Convert.ToByte(currentLine[28]);
                            npc.ModeHpTresholdOrItemVnum = Convert.ToInt16(currentLine[29]);
                            npc.ModeRangeTreshold = Convert.ToInt16(currentLine[30]);
                            npc.ModeCModeVnum = Convert.ToInt16(currentLine[31]);

                            npc.MinimumAttackRange = sbyte.Parse(currentLine[32]);
                            npc.MidgardDamage = Convert.ToInt16(currentLine[33]);
                            break;
                        }
                    case > 1 when currentLine[1] == "CARD":
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                byte type = (byte)int.Parse(currentLine[2 + 5 * i]);
                                if (type is 0 or 255)
                                {
                                    continue;
                                }

                                int first = int.Parse(currentLine[3 + 5 * i]);
                                int second = int.Parse(currentLine[4 + 5 * i]);

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

                                var itemCard = new BCardDTO
                                {
                                    NpcMonsterVNum = npc.Id,
                                    Type = type,
                                    SubType = (byte)((int.Parse(currentLine[5 + 5 * i]) + 1) * 10 + 1 + (first >= 0 ? 0 : 1)),
                                    FirstDataScalingType = (BCardScalingType)firstModulo,
                                    SecondDataScalingType = (BCardScalingType)secondModulo,
                                    FirstData = (int)Math.Abs(Math.Floor(first / 4.0)),
                                    SecondData = (int)Math.Abs(Math.Floor(second / 4.0)),
                                    CastType = byte.Parse(currentLine[6 + 5 * i]),
                                    TriggerType = i switch
                                    {
                                        0 => BCardNpcMonsterTriggerType.ON_FIRST_ATTACK,
                                        1 => BCardNpcMonsterTriggerType.ON_DEATH,
                                        _ => BCardNpcMonsterTriggerType.ON_DEATH // Custom
                                    }
                                };

                                npc.BCards.Add(itemCard);
                            }

                            break;
                        }
                    case > 1 when currentLine[1] == "BASIC":
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                byte type = (byte)int.Parse(currentLine[5 * i + 2]);
                                if (type == 0)
                                {
                                    continue;
                                }

                                int first = int.Parse(currentLine[3 + 5 * i]);
                                int second = int.Parse(currentLine[4 + 5 * i]);

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

                                var itemCard = new BCardDTO
                                {
                                    NpcMonsterVNum = npc.Id,
                                    Type = type,
                                    SubType = (byte)((int.Parse(currentLine[5 + 5 * i]) + 1) * 10 + 1 + (first >= 0 ? 0 : 1)),
                                    FirstDataScalingType = (BCardScalingType)firstModulo,
                                    SecondDataScalingType = (BCardScalingType)secondModulo,
                                    FirstData = (int)Math.Abs(Math.Floor(first / 4.0)),
                                    SecondData = (int)Math.Abs(Math.Floor(second / 4.0)),
                                    CastType = byte.Parse(currentLine[6 + 5 * i]),
                                    NpcTriggerType = (i % 2) switch
                                    {
                                        0 => BCardNpcTriggerType.ON_ATTACK,
                                        1 => BCardNpcTriggerType.ON_DEFENSE,
                                        _ => null
                                    }
                                };

                                npc.BCards.Add(itemCard);
                            }

                            break;
                        }
                    case > 3 when currentLine[1] == "ITEM":
                        {
                            _npcMonsters.Add(npc);
                            counter++;

                            for (int i = 2; i < currentLine.Length - 3; i += 3)
                            {
                                short vnum = Convert.ToInt16(currentLine[i]);
                                if (vnum is -1 or 0)
                                {
                                    continue;
                                }

                                npc.Drops ??= new List<DropDTO>();

                                // add to monster vnum
                                npc.Drops.Add(new DropDTO
                                {
                                    ItemVNum = vnum,
                                    Amount = Convert.ToInt32(currentLine[i + 2]),
                                    MonsterVNum = npc.Id,
                                    DropChance = Convert.ToInt32(currentLine[i + 1])
                                });
                            }

                            itemAreaBegin = false;
                            break;
                        }
                }
            }

            Log.Info($"[RESOURCE_LOADER] {counter.ToString()} Monster Data loaded");
            return _npcMonsters;
        }

        private static int GetModifier(NpcMonsterDto npc)
        {
            return npc.AttackType switch
            {
                AttackType.Melee => npc.MeleeHpFactor,
                AttackType.Ranged => npc.RangeDodgeFactor,
                AttackType.Magical => npc.MagicMpFactor
            };
        }

        private enum MobFlag : long
        {
            CANT_WALK = 1,
            CAN_BE_COLLECTED = 2,
            CANT_BE_DEBUFFED = 4,
            CAN_BE_CAUGHT = 8,
            DISSAPPEAR_AFTER_SECONDS = 16,
            DISSAPPEAR_AFTER_SECONDS_2 = 268435456,
            DISSAPPEAR_AFTER_HITTING = 32,
            HAS_MODE = 64,
            DISSAPPEAR_AFTER_SECONDS_MANA = 128,
            ON_DEFENSE_ONLY_ONCE = 256,
            HAS_DASH = 512,
            CAN_REGEN_MP = 1024,
            CAN_BE_PUSHED = 2048
        }
    }
}