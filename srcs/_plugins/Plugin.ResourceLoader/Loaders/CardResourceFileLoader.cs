using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Data.GameData;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Buffs;
using WingsEmu.Game._enum;
using WingsEmu.Game.Buffs;
using WingsEmu.Packets.Enums;

namespace Plugin.ResourceLoader.Loaders
{
    public class CardResourceFileLoader : IResourceLoader<CardDTO>
    {
        private readonly ResourceLoadingConfiguration _config;

        public CardResourceFileLoader(ResourceLoadingConfiguration config) => _config = config;

        public async Task<IReadOnlyList<CardDTO>> LoadAsync()
        {
            string filePath = Path.Combine(_config.GameDataPath, "Card.dat");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} should be present");
            }


            var card = new CardDTO();
            var cards = new List<CardDTO>();

            int counter = 0;
            bool itemAreaBegin = false;

            using var npcIdStream = new StreamReader(filePath, Encoding.GetEncoding(1252));
            string line;
            while ((line = await npcIdStream.ReadLineAsync()) != null)
            {
                string[] currentLine = line.Split('\t');

                switch (currentLine.Length)
                {
                    case > 2 when currentLine[1] == "VNUM":
                        card = new CardDTO
                        {
                            Id = Convert.ToInt16(currentLine[2])
                        };
                        itemAreaBegin = true;
                        break;
                    case > 2 when currentLine[1] == "NAME":
                        card.Name = currentLine[2];
                        break;
                    case > 3 when currentLine[1] == "GROUP":
                    {
                        if (!itemAreaBegin)
                        {
                            continue;
                        }

                        card.GroupId = Convert.ToInt32(currentLine[2]);
                        card.Level = Convert.ToByte(currentLine[3]);
                        break;
                    }
                    case > 3 when currentLine[1] == "STYLE":
                        card.BuffCategory = (BuffCategory)byte.Parse(currentLine[2]);
                        card.BuffType = Convert.ToByte(currentLine[3]);
                        card.ElementType = Convert.ToByte(currentLine[4]);
                        card.IsConstEffect = currentLine[5] == "1";
                        card.BuffPartnerLevel = Convert.ToByte(currentLine[6]);
                        break;
                    case > 3 when currentLine[1] == "EFFECT":
                        card.EffectId = Convert.ToInt32(currentLine[2]);
                        break;
                    case > 3 when currentLine[1] == "TIME":
                        card.Duration = Convert.ToInt32(currentLine[2]);
                        card.SecondBCardsDelay = Convert.ToInt32(currentLine[3]);
                        break;
                    default:
                    {
                        BCardDTO bCard;
                        switch (currentLine.Length)
                        {
                            case > 3 when currentLine[1] == "1ST":
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    if (currentLine[2 + i * 6] == "-1" || currentLine[2 + i * 6] == "0")
                                    {
                                        continue;
                                    }

                                    int first = int.Parse(currentLine[6 + i * 6]);
                                    int second = int.Parse(currentLine[7 + i * 6]);

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

                                    double tickPeriod = double.Parse(currentLine[5 + i * 6]);
                                    bCard = new BCardDTO
                                    {
                                        CardId = card.Id,
                                        Type = int.Parse(currentLine[2 + i * 6]),
                                        SubType = (byte)((Convert.ToByte(currentLine[3 + i * 6]) + 1) * 10 + 1 + (first < 0 ? 1 : 0)),
                                        FirstData = (int)Math.Abs(Math.Floor(first / 4.0)),
                                        SecondData = (int)Math.Abs(Math.Floor(second / 4.0)),
                                        ProcChance = int.Parse(currentLine[4 + i * 6]),
                                        TickPeriod = tickPeriod == 0 ? null : tickPeriod * 2,
                                        FirstDataScalingType = (BCardScalingType)firstModulo,
                                        SecondDataScalingType = (BCardScalingType)secondModulo,
                                        IsSecondBCardExecution = false
                                    };
                                    
                                    switch (bCard.Type)
                                    {
                                        case (byte)BCardType.DealDamageAround when bCard.SubType == (byte)AdditionalTypes.DealDamageAround.DealAreaDamagePerSecond:
                                        case (byte)BCardType.DealDamageAround when bCard.SubType == (byte)AdditionalTypes.DealDamageAround.DealAreaDamagePerSecondNegated:
                                        case (byte)BCardType.DealDamageAround when bCard.SubType == (byte)AdditionalTypes.DealDamageAround.EffectiveOnEnemyInAreaPerSecond:
                                        case (byte)BCardType.DealDamageAround when bCard.SubType == (byte)AdditionalTypes.DealDamageAround.EffectiveOnAllieInAreaPerSecond:
                                            bCard.TickPeriod = 1.5;
                                            break;
                                        case (byte)BCardType.TokenBasedAbilities when bCard.SubType == (byte)AdditionalTypes.TokenBasedAbilities.TokenGaugeIncreased:
                                            bCard.TickPeriod = 2;
                                            break;
                                        
                                        case (byte)BCardType.ConditionalEffects when bCard.SubType == (byte)AdditionalTypes.ConditionalEffects.ConsumeFuelPointsIncreaseDamage:
                                            bCard.TickPeriod = 4;
                                            break;
                                    }

                                    card.Bcards.Add(bCard);
                                }

                                break;
                            }
                            case > 3 when currentLine[1] == "2ST":
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    if (currentLine[2 + i * 6] == "-1" || currentLine[2 + i * 6] == "0")
                                    {
                                        continue;
                                    }

                                    int first = int.Parse(currentLine[6 + i * 6]);
                                    int second = int.Parse(currentLine[7 + i * 6]);

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

                                    double tickPeriod = double.Parse(currentLine[5 + i * 6]);
                                    bCard = new BCardDTO
                                    {
                                        CardId = card.Id,
                                        Type = byte.Parse(currentLine[2 + i * 6]),
                                        SubType = (byte)((Convert.ToByte(currentLine[3 + i * 6]) + 1) * 10 + 1 + (first < 0 ? 1 : 0)),
                                        FirstData = (int)Math.Abs(Math.Floor(first / 4.0)),
                                        SecondData = (int)Math.Abs(Math.Floor(second / 4.0)),
                                        ProcChance = int.Parse(currentLine[4 + i * 6]),
                                        TickPeriod = tickPeriod == 0 ? null : tickPeriod * 2,
                                        FirstDataScalingType = (BCardScalingType)firstModulo,
                                        SecondDataScalingType = (BCardScalingType)secondModulo,
                                        IsSecondBCardExecution = true
                                    };
                                    
                                    if ((bCard.Type == (byte)BCardType.DealDamageAround && bCard.SubType == 
                                            (byte)AdditionalTypes.DealDamageAround.DealAreaDamagePerSecond) ||
                                        (bCard.Type == (byte)BCardType.DealDamageAround && bCard.SubType == 
                                            (byte)AdditionalTypes.DealDamageAround.DealAreaDamagePerSecondNegated) ||
                                        (bCard.Type == (byte)BCardType.DealDamageAround && bCard.SubType == 
                                            (byte)AdditionalTypes.DealDamageAround.EffectiveOnEnemyInAreaPerSecond) ||
                                        (bCard.Type == (byte)BCardType.DealDamageAround && bCard.SubType == 
                                            (byte)AdditionalTypes.DealDamageAround.EffectiveOnAllieInAreaPerSecond))
                                    {
                                        bCard.TickPeriod = 1.5;
                                    }

                                    card.Bcards.Add(bCard);
                                }

                                break;
                            }
                            case > 3 when currentLine[1] == "LAST":
                                card.TimeoutBuff = short.Parse(currentLine[2]);
                                card.TimeoutBuffChance = byte.Parse(currentLine[3]);
                                itemAreaBegin = false;
                                cards.Add(card);
                                counter++;
                                break;
                        }

                        break;
                    }
                }
            }

            Log.Info($"[RESOURCE_LOADER] {cards.Count.ToString()} act desc loaded");
            return cards;
        }
    }
}