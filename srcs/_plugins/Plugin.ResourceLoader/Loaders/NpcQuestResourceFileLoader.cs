using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Data.GameData;
using WingsEmu.DTOs.Quests;

namespace Plugin.ResourceLoader.Loaders
{
    public class NpcQuestResourceFileLoader : IResourceLoader<QuestNpcDto>
    {
        private readonly ResourceLoadingConfiguration _configuration;

        public NpcQuestResourceFileLoader(ResourceLoadingConfiguration configuration) => _configuration = configuration;

        public async Task<IReadOnlyList<QuestNpcDto>> LoadAsync()
        {
            string filePath = Path.Combine(_configuration.GameDataPath, "qstnpc.dat");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} should be present");
            }

            var npcQuests = new List<QuestNpcDto>();
            using var idStream = new StreamReader(filePath, Encoding.GetEncoding(1252));
            string line;
            int counter = 0;
            int idCounter = 1;

            while ((line = idStream.ReadLine()) != null)
            {
                string[] currentLine = line.Split(' ');

                if (currentLine.Length < 5 || line.StartsWith('#'))
                {
                    continue;
                }

                counter++;
                var dto = new QuestNpcDto
                {
                    Id = idCounter++,
                    NpcVnum = short.Parse(currentLine[0]),
                    Level = short.Parse(currentLine[4])
                };

                bool isMainQuest = short.Parse(currentLine[1]) == 0;
                if (isMainQuest)
                {
                    dto.IsMainQuest = true;
                    dto.StartingScript = short.Parse(currentLine[2]);
                    dto.RequiredCompletedScript = short.Parse(currentLine[3]);
                    dto.MapId = short.Parse(currentLine[5]);
                }
                else
                {
                    dto.QuestId = int.Parse(currentLine[2]);
                    dto.IsMainQuest = false;
                }

                npcQuests.Add(dto);
            }

            Log.Info($"[RESOURCE_LOADER] {npcQuests.Count.ToString()} NPC quests loaded");
            return npcQuests;
        }
    }
}