using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Data.GameData;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace Plugin.ResourceLoader.Loaders
{
    public class ItemResourceFileLoader : IResourceLoader<ItemDTO>
    {
        private const string FILE_NAME = "Item.dat";

        private readonly ResourceLoadingConfiguration _configuration;

        private readonly List<ItemDTO> _itemDtos = new();

        public ItemResourceFileLoader(ResourceLoadingConfiguration configuration) => _configuration = configuration;

        public async Task<IReadOnlyList<ItemDTO>> LoadAsync()
        {
            if (_itemDtos.Any())
            {
                return _itemDtos;
            }

            string filePath = Path.Combine(_configuration.GameDataPath, FILE_NAME);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} should be present");
            }

            using var npcIdStream = new StreamReader(filePath, Encoding.GetEncoding(1252));

            var item = new ItemDTO();
            bool itemAreaBegin = false;
            int itemCounter = 0;

            string line;
            while ((line = await npcIdStream.ReadLineAsync()) != null)
            {
                try
                {
                    string[] currentLine = line.Split('\t');

                    switch (currentLine.Length)
                    {
                        case > 3 when currentLine[1] == "VNUM":
                            itemAreaBegin = true;
                            item.Id = int.Parse(currentLine[2]);
                            item.Price = long.Parse(currentLine[3]);
                            break;
                        case > 1 when currentLine[1] == "END":
                        {
                            if (!itemAreaBegin)
                            {
                                continue;
                            }

                            _itemDtos.Add(item);
                            itemCounter++;

                            item = new ItemDTO();
                            itemAreaBegin = false;
                            break;
                        }
                        case > 2 when currentLine[1] == "NAME":
                            item.Name = currentLine[2];
                            break;
                        case > 7 when currentLine[1] == "INDEX":
                            FillMorphAndIndexValues(currentLine, item);
                            break;
                        case > 3 when currentLine[1] == "TYPE":
                            // currentLine[2] 0-range 2-range 3-magic
                            item.AttackType = (AttackType)Convert.ToByte(currentLine[2]);
                            item.Class = item.EquipmentSlot == EquipmentType.Fairy ? (byte)15 : Convert.ToByte(currentLine[3]);
                            break;
                        case > 3 when currentLine[1] == "FLAG":
                            FillFlags(item, currentLine);
                            break;
                        case > 1 when currentLine[1] == "DATA":
                            FillData(item, currentLine);
                            break;
                        case > 1 when currentLine[1] == "BUFF":
                            FillBuff(currentLine, item);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Error while loading Item.dat", e);
                }
            }

            Log.Info($"[RESOURCE_LOADER] {itemCounter.ToString()} Items loaded");
            return _itemDtos;
        }

        private static void FillBuff(IReadOnlyList<string> currentLine, ItemDTO item)
        {
            for (int i = 0; i < 5; i++)
            {
                byte type = (byte)int.Parse(currentLine[2 + 5 * i]);
                if (type is 0 or 255) // 255 = -1
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
                    ItemVNum = item.Id,
                    Type = type,
                    SubType = (byte)((int.Parse(currentLine[5 + i * 5]) + 1) * 10 + 1 + (first < 0 ? 1 : 0)),
                    FirstDataScalingType = (BCardScalingType)firstModulo,
                    SecondDataScalingType = (BCardScalingType)secondModulo,
                    FirstData = (int)Math.Abs(Math.Floor(first / 4.0)),
                    SecondData = (int)Math.Abs(Math.Floor(second / 4.0)),
                    CastType = byte.Parse(currentLine[6 + 5 * i])
                };

                item.BCards.Add(itemCard);
            }
        }

        private static void FillFlags(ItemDTO item, IReadOnlyList<string> currentLine)
        {
            // useless [2]
            // useless [3]
            // useless [4]
            item.IsSoldable = currentLine[5] == "0";
            item.IsDroppable = currentLine[6] == "0";
            item.IsTradable = currentLine[7] == "0";
            item.IsMinilandActionable = currentLine[8] == "1";
            item.IsWarehouse = currentLine[9] == "1";
            item.ShowWarningOnUse = currentLine[10] == "1";
            item.IsTimeSpaceRewardBox = currentLine[11] == "1";
            item.ShowDescriptionOnHover = currentLine[12] == "1";
            item.Flag3 = currentLine[13] == "1";
            item.FollowMouseOnUse = currentLine[14] == "1";
            item.ShowSomethingOnHover = currentLine[15] == "1";
            item.IsColorable = currentLine[16] == "1";
            item.Sex = currentLine[18] == "1"
                ? (byte)1
                : currentLine[17] == "1"
                    ? (byte)2
                    : (byte)0;
            //not used item.Flag6 = currentLine[19] == "1";
            item.PlaySoundOnPickup = currentLine[20] == "1";
            item.UseReputationAsPrice = currentLine[21] == "1";
            if (item.UseReputationAsPrice)
            {
                item.ReputPrice = item.Price;
            }

            item.IsHeroic = currentLine[22] == "1";
            item.Flag7 = currentLine[23] == "1";
            item.IsLimited = currentLine[24] == "1";
        }

        private static void FillData(ItemDTO item, string[] currentLine)
        {
            item.Data = new int[20];

            for (int i = 0; i < 20; i++)
            {
                item.Data[i] = Convert.ToInt32(currentLine[2 + i]);
            }

            switch (item.ItemType)
            {
                case ItemType.Weapon:
                    item.LevelMinimum = Convert.ToByte(currentLine[2]);
                    item.DamageMinimum = Convert.ToInt16(currentLine[3]);
                    item.DamageMaximum = Convert.ToInt16(currentLine[4]);
                    item.HitRate = Convert.ToInt16(currentLine[5]);
                    item.CriticalLuckRate = Convert.ToSByte(currentLine[6]);
                    item.CriticalRate = Convert.ToInt16(currentLine[7]);
                    item.BasicUpgrade = Convert.ToByte(currentLine[10]);
                    item.MaximumAmmo = 100;
                    break;

                case ItemType.Armor:
                    item.LevelMinimum = Convert.ToByte(currentLine[2]);
                    item.CloseDefence = Convert.ToInt16(currentLine[3]);
                    item.DistanceDefence = Convert.ToInt16(currentLine[4]);
                    item.MagicDefence = Convert.ToInt16(currentLine[5]);
                    item.DefenceDodge = Convert.ToInt16(currentLine[6]);
                    item.DistanceDefenceDodge = Convert.ToInt16(currentLine[6]);
                    item.BasicUpgrade = Convert.ToByte(currentLine[10]);
                    break;

                case ItemType.Box:
                    item.Effect = Convert.ToInt16(currentLine[2]);
                    item.EffectValue = Convert.ToInt32(currentLine[3]);
                    item.LevelMinimum = Convert.ToByte(currentLine[4]);

                    if (item.ItemSubType == 7) // Magic Speed Booster
                    {
                        long time = Convert.ToInt32(currentLine[4]);
                        item.ItemValidTime = time == 0 ? -1 : Convert.ToInt32(currentLine[4]) * 3600;
                    }

                    break;

                case ItemType.Fashion:
                    item.LevelMinimum = Convert.ToByte(currentLine[2]);
                    item.CloseDefence = Convert.ToInt16(currentLine[3]);
                    item.DistanceDefence = Convert.ToInt16(currentLine[4]);
                    item.MagicDefence = Convert.ToInt16(currentLine[5]);
                    item.DefenceDodge = Convert.ToInt16(currentLine[6]);
                    item.DistanceDefenceDodge = Convert.ToInt16(currentLine[6]);

                    if (item.EquipmentSlot == EquipmentType.CostumeHat || item.EquipmentSlot == EquipmentType.CostumeSuit || item.EquipmentSlot == EquipmentType.WeaponSkin)
                    {
                        long time = Convert.ToInt32(currentLine[13]);
                        item.ItemValidTime = time == 0 ? -1 : Convert.ToInt32(currentLine[13]) * 3600;
                    }

                    break;

                case ItemType.Food:
                    item.Hp = Convert.ToInt16(currentLine[2]);
                    item.Mp = Convert.ToInt16(currentLine[4]);
                    break;

                case ItemType.Jewelry:
                    switch (item.EquipmentSlot)
                    {
                        case EquipmentType.Amulet:
                            item.LevelMinimum = Convert.ToByte(currentLine[2]);
                            item.ItemLeftType = Convert.ToInt16(currentLine[4]);
                            if (item.ItemLeftType == 100)
                            {
                                item.LeftUsages = Convert.ToInt32(currentLine[3]);
                            }
                            else if (item.ItemLeftType >= 1000)
                            {
                                item.ItemValidTime = Convert.ToInt64(currentLine[13]) * 3600;
                            }
                            else
                            {
                                item.ItemValidTime = Convert.ToInt64(currentLine[3]) == 0 ? -1 : Convert.ToInt64(currentLine[3]) / 10;
                            }

                            break;
                        case EquipmentType.Fairy:
                            item.Element = Convert.ToByte(currentLine[2]);
                            item.ElementRate = Convert.ToInt16(currentLine[3]);
                            if (item.Id <= 256)
                            {
                                item.MaxElementRate = 50;
                            }
                            else if (item.ElementRate == 0)
                            {
                                if (item.Id >= 800 && item.Id <= 804)
                                {
                                    item.MaxElementRate = 50;
                                }
                                else
                                {
                                    item.MaxElementRate = 70;
                                }
                            }
                            else if (item.ElementRate == 30)
                            {
                                item.MaxElementRate = 30;
                            }
                            else if (item.ElementRate == 35)
                            {
                                item.MaxElementRate = 35;
                            }
                            else if (item.ElementRate == 40)
                            {
                                item.MaxElementRate = 70;
                            }
                            else if (item.ElementRate == 50)
                            {
                                item.MaxElementRate = 80;
                            }

                            break;
                        
                        case EquipmentType.MiniPet:
                            item.LevelMinimum = Convert.ToByte(currentLine[2]);
                            item.ItemLeftType = Convert.ToInt16(currentLine[4]);
                            if (item.ItemLeftType >= 1000)
                            {
                                item.ItemValidTime = Convert.ToInt64(currentLine[13]);
                            }
                            break;
                        
                        default:
                            item.LevelMinimum = Convert.ToByte(currentLine[2]);
                            item.MaxCellonLvl = Convert.ToByte(currentLine[3]);
                            item.MaxCellon = Convert.ToByte(currentLine[4]);
                            break;
                    }

                    break;

                case ItemType.Event:
                    item.Effect = Convert.ToInt16(currentLine[2]);
                    item.EffectValue = Convert.ToInt16(currentLine[3]);
                    
                    switch (item.Id)
                    {
                        case 1585:
                        case 9044:
                            item.Effect = 800;
                            item.Data[2] = 859;
                            item.Data[3] = 1343;
                            break;
                        case 1983:
                        case 9059:
                            item.Effect = 800;
                            item.Data[2] = 875;
                            item.Data[3] = 1558;
                            break;
                        case 1984:
                        case 9060:
                            item.Effect = 800;
                            item.Data[2] = 876;
                            item.Data[3] = 1559;
                            break;
                        case 1985:
                        case 9061:
                            item.Effect = 800;
                            item.Data[2] = 877;
                            item.Data[3] = 1560;
                            break;
                        case 1986:
                        case 9062:
                            item.Effect = 800;
                            item.Data[2] = 878;
                            item.Data[3] = 1560;
                            break;
                        case 1987:
                        case 9063:
                            item.Effect = 800;
                            item.Data[2] = 879;
                            item.Data[3] = 1561;
                            break;
                        case 1988:
                        case 9064:
                            item.Effect = 800;
                            item.Data[2] = 880;
                            item.Data[3] = 1561;
                            break;
                    }
                    break;

                case ItemType.Special:
                    switch (item.Id)
                    {
                        case 9283:
                            item.Effect = 1919;
                            
                            break;
                        case 9581:
                            item.Effect = 667;
                            break;
                        case 5787:
                        case 9142:
                            item.Effect = 668;
                            break;
                        case 5853:
                            item.Effect = 1717;
                            item.EffectValue = 1;
                            break;
                        case 5854:
                            item.Effect = 1717;
                            item.EffectValue = 2;
                            break;
                        case 5855:
                            item.Effect = 1717;
                            item.EffectValue = 3;
                            break;
                        case 1272:
                        case 1858:
                        case 9047:
                            item.Effect = 1005;
                            item.EffectValue = 10;
                            break;

                        case 1273:
                        case 9024:
                            item.Effect = 1005;
                            item.EffectValue = 30;
                            break;

                        case 1274:
                        case 9025:
                            item.Effect = 1005;
                            item.EffectValue = 60;
                            break;

                        case 1279:
                        case 9029:
                            item.Effect = 1007;
                            item.EffectValue = 30;
                            break;

                        case 1280:
                        case 9030:
                            item.Effect = 1007;
                            item.EffectValue = 60;
                            break;

                        case 1923:
                        case 9056:
                            item.Effect = 1007;
                            item.EffectValue = 10;
                            break;

                        case 1275:
                        case 1886:
                        case 9026:
                            item.Effect = 1008;
                            item.EffectValue = 10;
                            break;

                        case 1276:
                        case 9027:
                            item.Effect = 1008;
                            item.EffectValue = 30;
                            break;

                        case 1277:
                        case 9028:
                            item.Effect = 1008;
                            item.EffectValue = 60;
                            break;

                        case 5060:
                        case 9066:
                            item.Effect = 1003;
                            item.EffectValue = 30;
                            break;

                        case 5061:
                        case 9067:
                            item.Effect = 1004;
                            item.EffectValue = 7;
                            break;

                        case 5062:
                        case 9068:
                            item.Effect = 1004;
                            item.EffectValue = 1;
                            break;
                        case 5115:
                            item.Effect = 652;
                            break;

                        case 1981:
                            item.Effect = 34; // imagined number as for I = √(-1), complex z = a + bi
                            break;

                        case 1982:
                            item.Effect = 6969; // imagined number as for I = √(-1), complex z = a + bi
                            break;

                        case 9071:
                        case 5119: // Speed booster
                            item.Effect = 998;
                            break;

                        case 180: // attack amulet
                            item.Effect = 932;
                            break;

                        case 181: // defense amulet
                            item.Effect = 933;
                            break;
                        
                        default:
                            if (item.Id > 5891 && item.Id < 5900 || item.Id > 9100 && item.Id < 9109)
                            {
                                item.Effect = 69; // imagined number as for I = √(-1), complex z = a + bi
                            }
                            else
                            {
                                item.Effect = Convert.ToInt16(currentLine[2]);
                            }

                            break;
                    }

                    switch (item.Effect)
                    {
                        case 305:
                            item.EffectValue = Convert.ToInt32(currentLine[5]);
                            item.Morph = Convert.ToInt16(currentLine[4]);
                            break;

                        default:
                            item.EffectValue = item.EffectValue == 0 ? Convert.ToInt32(currentLine[4]) : item.EffectValue;
                            break;
                    }

                    item.WaitDelay = 5000;
                    break;

                case ItemType.Magical:
                    item.Effect = Convert.ToInt16(currentLine[2]);

                    if (item.Effect == 99)
                    {
                        item.LevelMinimum = Convert.ToByte(currentLine[4]);
                        item.EffectValue = Convert.ToByte(currentLine[5]);
                    }
                    else
                    {
                        item.EffectValue = Convert.ToInt32(currentLine[4]);
                    }

                    break;

                case ItemType.Specialist:
                    item.IsPartnerSpecialist = item.ItemSubType == 4;
                    item.Speed = Convert.ToByte(currentLine[5]);
                    if (item.IsPartnerSpecialist)
                    {
                        item.Element = Convert.ToByte(currentLine[3]);
                        item.ElementRate = Convert.ToInt16(currentLine[4]);
                        item.PartnerClass = Convert.ToByte(currentLine[19]);
                        item.LevelMinimum = Convert.ToByte(currentLine[20]);
                    }
                    else
                    {
                        item.LevelJobMinimum = Convert.ToByte(currentLine[20]);
                        item.ReputationMinimum = Convert.ToByte(currentLine[21]);
                    }

                    item.SpPointsUsage = Convert.ToByte(currentLine[13]);
                    item.SpMorphId = item.IsPartnerSpecialist ? (byte)(1 + Convert.ToByte(currentLine[14])) : Convert.ToByte(currentLine[14]);
                    item.FireResistance = Convert.ToByte(currentLine[15]);
                    item.WaterResistance = Convert.ToByte(currentLine[16]);
                    item.LightResistance = Convert.ToByte(currentLine[17]);
                    item.DarkResistance = Convert.ToByte(currentLine[18]);

                    var elementdic = new Dictionary<int, int> { { 0, 0 } };
                    if (item.FireResistance != 0)
                    {
                        elementdic.Add(1, item.FireResistance);
                    }

                    if (item.WaterResistance != 0)
                    {
                        elementdic.Add(2, item.WaterResistance);
                    }

                    if (item.LightResistance != 0)
                    {
                        elementdic.Add(3, item.LightResistance);
                    }

                    if (item.DarkResistance != 0)
                    {
                        elementdic.Add(4, item.DarkResistance);
                    }

                    if (!item.IsPartnerSpecialist)
                    {
                        item.Element = (byte)elementdic.OrderByDescending(s => s.Value).First().Key;
                    }

                    // needs to be hardcoded
                    switch (item.Id)
                    {
                        case 4099:
                            item.Element = 1;
                            break;
                        
                        case 4562:
                        case 8429:
                            item.Element = 2;
                            break;
                        
                        case 8580:
                        case 8569:
                            item.Element = 4;
                            break;
                        
                        case 901:
                            item.Element = 1;
                            break;

                        case 903:
                            item.Element = 2;
                            break;

                        case 906:
                            item.Element = 3;
                            break;

                        case 909:
                            item.Element = 3;
                            break;
                    }

                    break;

                case ItemType.Shell:
                    byte shellType = Convert.ToByte(currentLine[5]);

                    item.ShellMinimumLevel = Convert.ToInt16(currentLine[3]);
                    item.ShellMaximumLevel = Convert.ToInt16(currentLine[4]);
                    item.ShellType = (ShellType)(item.ItemSubType == 1 ? shellType + 50 : shellType);
                    break;

                case ItemType.Main:
                    item.Effect = Convert.ToInt16(currentLine[2]);
                    item.EffectValue = Convert.ToInt32(currentLine[4]);
                    break;

                case ItemType.Upgrade:
                    item.Effect = Convert.ToInt16(currentLine[2]);
                    switch (item.Id)
                    {
                        // UpgradeItems (needed to be hardcoded)
                        case (int)ItemVnums.EQ_NORMAL_SCROLL:
                        case (int)ItemVnums.EQ_NORMAL_SCROLL_EVENT:
                            item.EffectValue = 26;
                            break;

                        case (int)ItemVnums.LOWER_SP_SCROLL:
                        case (int)ItemVnums.LOWER_SP_SCROLL_LIMITED:
                        case (int)ItemVnums.LOWER_SP_PROTECTION_SCROLL_PREMIUM:
                        case (int)ItemVnums.LOWER_SP_PROTECTION_SCROLL_PREMIUM_LIMITED:
                            item.EffectValue = 27;
                            break;

                        case (int)ItemVnums.HIGHER_SP_SCROLL:
                        case (int)ItemVnums.HIGHER_SP_SCROLL_LIMITED:
                        case (int)ItemVnums.HIGHER_SP_PROTECTION_SCROLL_PREMIUM:
                        case (int)ItemVnums.HIGHER_SP_PROTECTION_SCROLL_PREMIUM_LIMITED:
                            item.EffectValue = 28;
                            break;

                        case (int)ItemVnums.SCROLL_CHICKEN:
                            item.EffectValue = 47;
                            break;

                        case (int)ItemVnums.SCROLL_PYJAMA:
                            item.EffectValue = 50;
                            break;

                        case (int)ItemVnums.EQ_GOLD_SCROLL:
                            item.EffectValue = 61;
                            break;

                        case (int)ItemVnums.SCROLL_PIRATE:
                            item.EffectValue = 60;
                            break;

                        case (int)ItemVnums.ZENAS_EGGS:
                            item.EffectValue = 75;
                            break;

                        case (int)ItemVnums.ERENIA_EGGS:
                            item.EffectValue = 76;
                            break;

                        case (int)ItemVnums.FERNON_EGGS:
                            item.EffectValue = 77;
                            break;

                        case (int)ItemVnums.PREMIUM_RUNE_OF_FORTUNE_SCROLL:
                            item.EffectValue = 89;
                            break;

                        case (int)ItemVnums.RUNE_OF_FORTUNE_SCROLL:
                        case (int)ItemVnums.RUNE_OF_FORTUNE_SCROLL_EVENT_LIMITED:
                            item.EffectValue = 93;
                            break;
                        
                        case (int)ItemVnums.TATTOO_SCROLL:
                        case (int)ItemVnums.TATTOO_SAFEGUARD_SCROLL:
                        case (int)ItemVnums.TATTOO_SAFEGUARD_SCROLL_LIMITED:
                            item.EffectValue = 90;
                            break;
                        
                        case (int)ItemVnums.DRAGON_SP_SCROLL:
                        case (int)ItemVnums.DRAGON_SP_SCROLL_LIMITED:
                        case (int)ItemVnums.DRAGON_SP_PROTECTION_SCROLL_PREMIUM:
                        case (int)ItemVnums.DRAGON_SP_PROTECTION_SCROLL_PREMIUM_LIMITED:
                            item.EffectValue = 100;
                            break;
                        
                        case (int)ItemVnums.PREMIUM_RUNIC_UPGRADE_SCROLL:
                        case (int)ItemVnums.PREMIUM_RUNIC_UPGRADE_SCROLL_LIMITED:
                        case (int)ItemVnums.PREMIUM_RUNIC_UPGRADE_SCROLL_EVENT_LIMITED:
                            item.EffectValue = 107;
                            break;

                        default:
                            item.EffectValue = Convert.ToInt32(currentLine[4]);
                            break;
                    }

                    break;

                case ItemType.Production:
                    item.Effect = Convert.ToInt16(currentLine[2]);
                    item.EffectValue = Convert.ToInt32(currentLine[4]);
                    break;

                case ItemType.Map:
                    item.Effect = Convert.ToInt16(currentLine[2]);
                    item.EffectValue = Convert.ToInt32(currentLine[4]);
                    break;

                case ItemType.Potion:
                    item.Hp = Convert.ToInt16(currentLine[2]);
                    item.Mp = Convert.ToInt16(currentLine[4]);
                    break;

                case ItemType.Snack:
                    item.Hp = Convert.ToInt16(currentLine[2]);
                    item.Mp = Convert.ToInt16(currentLine[4]);
                    break;

                case ItemType.PetPartnerItem:
                    item.Effect = Convert.ToInt16(currentLine[2]);
                    item.EffectValue = Convert.ToInt32(currentLine[4]);
                    break;

                case ItemType.Material:
                case ItemType.Sell:
                case ItemType.Quest2:
                case ItemType.Quest1:
                case ItemType.Ammo:
                    // nothing to parse
                    break;
            }

            if (item.Type == InventoryType.Miniland)
            {
                item.MinilandObjectPoint = int.Parse(currentLine[2]);
                item.EffectValue = short.Parse(currentLine[8]);
                item.Width = Convert.ToByte(currentLine[9]) == 0 ? (byte)1 : Convert.ToByte(currentLine[9]);
                item.Height = Convert.ToByte(currentLine[10]) == 0 ? (byte)1 : Convert.ToByte(currentLine[10]);
            }

            if (item.EquipmentSlot != EquipmentType.Boots && item.EquipmentSlot != EquipmentType.Gloves || item.Type != 0)
            {
                return;
            }

            item.FireResistance = Convert.ToByte(currentLine[7]);
            item.WaterResistance = Convert.ToByte(currentLine[8]);
            item.LightResistance = Convert.ToByte(currentLine[9]);
            item.DarkResistance = Convert.ToByte(currentLine[11]);
        }

        private static void FillMorphAndIndexValues(string[] currentLine, ItemDTO item)
        {
            switch (Convert.ToByte(currentLine[2]))
            {
                case 4:
                    item.Type = InventoryType.Equipment;
                    break;

                case 8:
                    item.Type = InventoryType.Equipment;
                    break;

                case 9:
                    item.Type = InventoryType.Main;
                    break;

                case 10:
                    item.Type = InventoryType.Etc;
                    break;

                default:
                    item.Type = (InventoryType)Enum.Parse(typeof(InventoryType), currentLine[2]);
                    break;
            }

            item.ItemType = currentLine[3] != "-1" ? (ItemType)Enum.Parse(typeof(ItemType), $"{(short)item.Type}{currentLine[3]}") : ItemType.Weapon;
            item.ItemSubType = Convert.ToByte(currentLine[4]);
            item.EquipmentSlot = (EquipmentType)Enum.Parse(typeof(EquipmentType), currentLine[5] != "-1" ? currentLine[5] : "0");

            item.IconId = Convert.ToInt32(currentLine[6]);
            switch (item.Id)
            {
                case 4101:
                case 4102:
                case 4103:
                case 4104:
                case 4105:
                    item.EquipmentSlot = 0;
                    break;

                default:
                    if (item.EquipmentSlot.Equals(EquipmentType.Amulet))
                    {
                        switch (item.Id)
                        {
                            case 4503:
                                item.EffectValue = 4544;
                                break;

                            case 4504:
                                item.EffectValue = 4294;
                                break;

                            default:
                                item.EffectValue = Convert.ToInt16(currentLine[7]);
                                break;
                        }
                    }
                    else
                    {
                        item.Morph = Convert.ToInt16(currentLine[7]);
                    }

                    break;
            }
        }
    }
}