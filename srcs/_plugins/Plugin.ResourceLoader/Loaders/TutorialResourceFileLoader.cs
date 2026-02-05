using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Data.GameData;
using WingsEmu.DTOs.Quests;

namespace Plugin.ResourceLoader.Loaders
{
    public class TutorialResourceFileLoader : IResourceLoader<TutorialDto>
    {
        private readonly ResourceLoadingConfiguration _configuration;

        public TutorialResourceFileLoader(ResourceLoadingConfiguration configuration) => _configuration = configuration;

        public async Task<IReadOnlyList<TutorialDto>> LoadAsync()
        {
            string filePath = Path.Combine(_configuration.GameDataPath, "tutorial.dat");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} should be present");
            }

            var scriptDatas = new List<TutorialDto>();
            using var tutorialIdStream = new StreamReader(filePath, Encoding.GetEncoding(1252));
            string line;
            int scriptId = 1;
            int tutorialId = 1;
            char[] splits = { ' ', '\t' };

            while ((line = tutorialIdStream.ReadLine()) != null)
            {
                string[] currentLine = line.Split(splits, StringSplitOptions.RemoveEmptyEntries);

                if (currentLine.Length < 2 && !currentLine.Contains("end"))
                {
                    continue;
                }

                if (currentLine[0] == "end")
                {
                    scriptId++;
                    continue;
                }

                if (!int.TryParse(currentLine[0], out int index))
                {
                    continue;
                }


                string[] scriptIndexData = new string[currentLine.Length - 1];
                Array.Copy(currentLine, 1, scriptIndexData, 0, currentLine.Length - 1);

                if (scriptIndexData.Length < 2 && !scriptIndexData.Contains("targetoff"))
                {
                    continue;
                }

                string actionType = scriptIndexData[0];

                TutorialActionType type = actionType switch
                {
                    "talk" => TutorialActionType.TALK,
                    "quest" => TutorialActionType.START_QUEST,
                    "web" => TutorialActionType.WEB_DISPLAY,
                    "q_complete" => TutorialActionType.WAIT_FOR_QUEST_COMPLETION,
                    "openwin" => TutorialActionType.OPEN_WINDOW,
                    "q_pay" => TutorialActionType.WAIT_FOR_REWARDS_CLAIM,
                    "run" => TutorialActionType.RUN,
                    "time" => TutorialActionType.DELAY,
                    "target" => TutorialActionType.SHOW_TARGET,
                    "targetoff" => TutorialActionType.REMOVE_TARGET,
                    _ => TutorialActionType.NONE
                };

                int data = 0;
                if (type != TutorialActionType.REMOVE_TARGET)
                {
                    int.TryParse(scriptIndexData[1], out data);
                }

                var dto = new TutorialDto
                {
                    Id = tutorialId++,
                    ScriptId = scriptId,
                    ScriptIndex = index,
                    Type = type,
                    Data = data
                };

                scriptDatas.Add(dto);
            }

            Log.Info($"[RESOURCE_LOADER] {scriptDatas.Count.ToString()} Tutorial Scripts loaded");
            return scriptDatas;
        }
    }
}