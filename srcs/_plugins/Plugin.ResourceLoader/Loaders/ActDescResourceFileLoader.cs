using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Data.ActDesc;
using WingsAPI.Data.GameData;

namespace Plugin.ResourceLoader.Loaders
{
    public class ActDescResourceFileLoader : IResourceLoader<ActDescDTO>
    {
        private readonly ResourceLoadingConfiguration _configuration;

        public ActDescResourceFileLoader(ResourceLoadingConfiguration configuration) => _configuration = configuration;

        public async Task<IReadOnlyList<ActDescDTO>> LoadAsync()
        {
            string filePath = Path.Combine(_configuration.GameDataPath, "act_desc.dat");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} should be present");
            }

            var actDescDatas = new List<ActDescDTO>();
            var actNames = new Dictionary<byte, string>();

            using var actDescStream = new StreamReader(filePath, Encoding.GetEncoding(1252));
            string line;
            char[] splits = { ' ', '\t' };

            while ((line = actDescStream.ReadLine()) != null)
            {
                string[] currentLine = line.Split(splits, StringSplitOptions.RemoveEmptyEntries);

                if (currentLine[0] != "Data" && currentLine[0] != "A")
                {
                    continue;
                }

                if (currentLine[0] == "Data")
                {
                    var dto = new ActDescDTO
                    {
                        Act = byte.Parse(currentLine[2]),
                        SubAct = byte.Parse(currentLine[3]),
                        TsAmount = byte.Parse(currentLine[4])
                    };

                    actDescDatas.Add(dto);
                }

                if (currentLine[0] == "A")
                {
                    actNames.TryAdd(byte.Parse(currentLine[1]), currentLine[2]);
                }
            }

            foreach (ActDescDTO actDescDto in actDescDatas)
            {
                actDescDto.ActName = actNames.GetValueOrDefault(actDescDto.Act);
            }

            Log.Info($"[RESOURCE_LOADER] {actDescDatas.Count.ToString()} act desc loaded");
            return actDescDatas;
        }
    }
}