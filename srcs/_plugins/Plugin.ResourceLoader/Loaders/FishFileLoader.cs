using PhoenixLib.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingsAPI.Data.Fish;
using WingsAPI.Data.GameData;

namespace Plugin.ResourceLoader.Loaders
{
    public class FishFileLoader : IResourceLoader<FishingSpotDto>
    {
        private readonly ResourceLoadingConfiguration _configuration;

        public FishFileLoader(ResourceLoadingConfiguration configuration) => _configuration = configuration;

        public async Task<IReadOnlyList<FishingSpotDto>> LoadAsync()
        {
            string filePath = Path.Combine(_configuration.GameDataPath, "fish.dat");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} should be present");
            }

            var fishingSpotDatas = new List<FishingSpotDto>();
            var fishDto = new FishingSpotDto();
            var rewardsList = new List<FishingRewardsDto>();
            long mapId = 0;
            int min = 0, max = 0, vnum = 0, basicEnd = 0;
            using var fileStream = new StreamReader(filePath, Encoding.GetEncoding(1252));
            string line;
            while ((line = await fileStream.ReadLineAsync()) != null)
            {
                string[] currentLine = line.Split('\t');
                if (currentLine.Length <= 1 && currentLine[0] != "#=======") continue;

                switch (currentLine[0])
                {
                    case "BASICT":
                        basicEnd = Convert.ToInt32(currentLine[1]);
                        break;

                    case "POS":
                        fishDto.Paths.Add(new()
                        {
                            X = Convert.ToInt32(currentLine[3]),
                            Y = Convert.ToInt32(currentLine[4]),
                            Dir = Convert.ToInt32(currentLine[5]),
                            MapId = mapId,
                        });
                        break;

                    case "VNUM":
                        vnum = Convert.ToInt32(currentLine[1]);
                        break;

                    case "LEVEL":

                        max = Convert.ToInt32(currentLine[2]);
                        min = Convert.ToInt32(currentLine[1]);

                        break;

                    case "ITEM":
                    case "BASIC":

                        double percent = Convert.ToDouble(currentLine[3]) / 100;
                        rewardsList.Add(new FishingRewardsDto
                        {
                            IsMaterial = currentLine[0] == "BASIC",
                            RewardsPercent = percent,
                            RewardsVnum = Convert.ToInt16(currentLine[2]),
                            FishVnum = vnum,
                        });
                        break;

                    case "MAP":
                        mapId = Convert.ToInt32(currentLine[2]);
                        fishDto = new FishingSpotDto
                        {
                            FishVnum = vnum,
                            MinLvl = min,
                            MaxLvl = max,
                            MapId = mapId,
                        };
                        fishingSpotDatas.Add(fishDto);
                        break;
                }
            }

            // fill rewards 
            foreach (FishingSpotDto spot in fishingSpotDatas)
            {
                spot.Rewards.AddRange(rewardsList.Where(s => s.FishVnum == spot.FishVnum));
            }

            Log.Info($"[RESOURCE_LOADER] {fishingSpotDatas.Count} Fish spot loaded");
            return fishingSpotDatas;
        }
    }
}
