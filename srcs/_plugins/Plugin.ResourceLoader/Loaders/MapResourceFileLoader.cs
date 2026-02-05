using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Data.GameData;
using WingsEmu.DTOs.Maps;

namespace Plugin.ResourceLoader.Loaders
{
    public class MapResourceFileLoader : IResourceLoader<MapDataDTO>
    {
        private readonly ResourceLoadingConfiguration _config;


        public MapResourceFileLoader(ResourceLoadingConfiguration config) => _config = config;

        public async Task<IReadOnlyList<MapDataDTO>> LoadAsync()
        {
            string filePath = Path.Combine(_config.GameDataPath, "MapIDData.dat");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} should be present");
            }

            var maps = new List<MapDataDTO>();
            var dictionaryId = new Dictionary<int, string>();

            int i = 0;
            using (var mapIdStream = new StreamReader(filePath, Encoding.GetEncoding(1252)))
            {
                string line;
                while ((line = await mapIdStream.ReadLineAsync()) != null)
                {
                    string[] values = line.Split(' ');
                    if (values.Length <= 1)
                    {
                        continue;
                    }

                    if (!int.TryParse(values[0], out int mapId))
                    {
                        continue;
                    }

                    if (!dictionaryId.ContainsKey(mapId))
                    {
                        dictionaryId.Add(mapId, values[4]);
                    }
                }
            }

            foreach (FileInfo file in new DirectoryInfo(_config.GameMapsPath).GetFiles())
            {
                string name = string.Empty;

                if (dictionaryId.TryGetValue(int.Parse(file.Name), out string value))
                {
                    name = value;
                }


                byte[] data = await File.ReadAllBytesAsync(file.FullName);
                short width = BitConverter.ToInt16(data, 0);
                short height = BitConverter.ToInt16(data, 2);

                maps.Add(new MapDataDTO
                {
                    Id = short.Parse(file.Name),
                    Name = name,
                    Width = width,
                    Height = height,
                    Grid = data.Skip(4).ToArray()
                });
                i++;
            }

            Log.Info($"[RESOURCE_LOADER] {maps.Count} Maps loaded");
            return maps;
        }
    }
}