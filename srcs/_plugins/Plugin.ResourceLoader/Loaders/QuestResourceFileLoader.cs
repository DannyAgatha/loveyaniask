using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloneExtensions;
using PhoenixLib.Logging;
using WingsAPI.Data.GameData;
using WingsEmu.DTOs.Quests;
using WingsEmu.Packets.Enums;

namespace Plugin.ResourceLoader.Loaders
{
    public class QuestResourceFileLoader : IResourceLoader<QuestDto>
    {
        private readonly ResourceLoadingConfiguration _configuration;
        private readonly List<QuestDto> _quests = new();

        public QuestResourceFileLoader(ResourceLoadingConfiguration configuration) => _configuration = configuration;

        public async Task<IReadOnlyList<QuestDto>> LoadAsync()
        {
            if (_quests.Any())
            {
                return _quests;
            }

            string fileQuestPath = Path.Combine(_configuration.GameDataPath, "quest.dat");
            string fileRewardsPath = Path.Combine(_configuration.GameDataPath, "qstprize.dat");

            if (!File.Exists(fileQuestPath))
            {
                throw new FileNotFoundException($"{fileQuestPath} should be present");
            }

            if (!File.Exists(fileRewardsPath))
            {
                throw new FileNotFoundException($"{fileRewardsPath} should be present");
            }

            var dictionaryRewards = new Dictionary<long, QuestPrizeDto>();
            string line;

            FillRewards(fileRewardsPath, dictionaryRewards);

            // Current
            var quest = new QuestDto();

            byte objectiveIndex = 0;
            using var questStream = new StreamReader(fileQuestPath, Encoding.GetEncoding(1252));
            while ((line = await questStream.ReadLineAsync()) != null)
            {
                string[] currentLine = line.Split('\t');
                if (currentLine.Length > 1 || currentLine[0] == "END")
                {
                    switch (currentLine[0])
                    {
                        case "VNUM":
                            quest = new QuestDto
                            {
                                Id = int.Parse(currentLine[1]),
                                QuestType = (QuestType)int.Parse(currentLine[2]),
                                AutoFinish = Convert.ToInt32(currentLine[3]) == 0,
                                Unknown1 = Convert.ToInt32(currentLine[4]),
                                RequiredQuestId = Convert.ToInt32(currentLine[5]),
                                IsBlue = Convert.ToInt32(currentLine[6]) == 1
                            };

                            objectiveIndex = 0;
                            break;

                        case "LINK":
                            quest.NextQuestId = int.Parse(currentLine[1]);
                            break;

                        case "LEVEL":
                            quest.MinLevel = byte.Parse(currentLine[1]);
                            quest.MaxLevel = byte.Parse(currentLine[2]);
                            break;

                        case "TALK":
                            quest.DialogStarting = int.Parse(currentLine[1]);
                            quest.DialogFinish = int.Parse(currentLine[2]);
                            quest.TalkerVnum = int.Parse(currentLine[3]);
                            quest.DialogDuring = int.Parse(currentLine[4]);
                            break;

                        case "TARGET":
                            quest.TargetMapX = short.Parse(currentLine[1]);
                            quest.TargetMapY = short.Parse(currentLine[2]);
                            quest.TargetMapId = short.Parse(currentLine[3]);
                            break;

                        case "TITLE":
                            quest.Name = currentLine[1];
                            break;

                        case "DESC":
                            quest.Description = currentLine[1];
                            break;

                        case "DATA":
                            objectiveIndex++;
                            int data0 = int.Parse(currentLine[1]);
                            int data1 = int.Parse(currentLine[2]);
                            int data2 = int.Parse(currentLine[3]);
                            int data3 = int.Parse(currentLine[4]);
                            quest.Objectives.Add(new QuestObjectiveDto
                            {
                                Data0 = data0,
                                Data1 = data1,
                                Data2 = data2,
                                Data3 = data3,
                                ObjectiveIndex = objectiveIndex,
                                QuestId = quest.Id
                            });
                            break;

                        case "PRIZE":
                            for (int a = 1; a < 5; a++)
                            {
                                if (!dictionaryRewards.ContainsKey(long.Parse(currentLine[a])))
                                {
                                    continue;
                                }

                                QuestPrizeDto currentReward = dictionaryRewards[long.Parse(currentLine[a])].GetClone();
                                currentReward.QuestId = quest.Id;
                                quest.Prizes.Add(currentReward);
                            }

                            break;

                        case "END":
                            _quests.Add(quest);
                            break;
                    }
                }
            }

            questStream.Close();
            Log.Info($"[RESOURCE_LOADER] {_quests.Count.ToString()} Quests loaded");
            return _quests;
        }

        private void FillRewards(string fileRewardsPath, Dictionary<long, QuestPrizeDto> dictionaryRewards)
        {
            using var questRewardStream = new StreamReader(fileRewardsPath, Encoding.GetEncoding(1252));
            string line;
            var reward = new QuestPrizeDto();
            int currentRewardId = 0;
            while ((line = questRewardStream.ReadLine()) != null)
            {
                string[] currentLine = line.Split('\t');
                if (currentLine.Length <= 1 && currentLine[0] != "END")
                {
                    continue;
                }

                switch (currentLine[0])
                {
                    case "VNUM":
                        reward = new QuestPrizeDto
                        {
                            RewardType = byte.Parse(currentLine[2])
                        };
                        currentRewardId = int.Parse(currentLine[1]);
                        break;

                    case "DATA":
                        reward.Data0 = int.Parse(currentLine[1]);
                        reward.Data1 = int.Parse(currentLine[2]);
                        reward.Data2 = int.Parse(currentLine[3]);
                        reward.Data3 = int.Parse(currentLine[4]);
                        reward.Data4 = int.Parse(currentLine[5]);

                        break;

                    case "END":
                        dictionaryRewards[currentRewardId] = reward;
                        break;
                }
            }

            questRewardStream.Close();
        }
    }
}