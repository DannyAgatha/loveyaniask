using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using PhoenixLib.MultiLanguage;
using WingsAPI.Data.GameData;
using WingsEmu.DTOs.Buffs;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.NpcMonster;
using WingsEmu.DTOs.Quests;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._i18n;

namespace Plugin.ResourceLoader.Loaders
{
    public class GameDataLanguageFileLoader : IResourceLoader<GameDataTranslationDto>
    {
        private static readonly List<(string, GameDataType)> _fileNames = new()
        {
            new ValueTuple<string, GameDataType>("_code_{0}_Card.txt", GameDataType.Card),
            new ValueTuple<string, GameDataType>("_code_{0}_monster.txt", GameDataType.NpcMonster),
            new ValueTuple<string, GameDataType>("_code_{0}_Item.txt", GameDataType.Item),
            new ValueTuple<string, GameDataType>("_code_{0}_quest.txt", GameDataType.QuestName),
            new ValueTuple<string, GameDataType>("_code_{0}_Skill.txt", GameDataType.Skill),
            new ValueTuple<string, GameDataType>("_code_{0}_MapIDData.txt", GameDataType.Map)
        };

        private readonly IResourceLoader<CardDTO> _cardLoader;

        private readonly ResourceLoadingConfiguration _config;
        private readonly IResourceLoader<ItemDTO> _itemLoader;
        private readonly IResourceLoader<NpcMonsterDto> _npcLoader;
        private readonly IResourceLoader<QuestDto> _questLoader;
        private readonly IResourceLoader<SkillDTO> _skillLoader;
        private readonly IResourceLoader<MapDataDTO> _mapLoader;

        public GameDataLanguageFileLoader(ResourceLoadingConfiguration config, IResourceLoader<ItemDTO> itemLoader, IResourceLoader<CardDTO> cardLoader, IResourceLoader<SkillDTO> skillLoader,
            IResourceLoader<QuestDto> questLoader, IResourceLoader<NpcMonsterDto> npcLoader, IResourceLoader<MapDataDTO> mapLoader)
        {
            _config = config;
            _itemLoader = itemLoader;
            _cardLoader = cardLoader;
            _skillLoader = skillLoader;
            _questLoader = questLoader;
            _npcLoader = npcLoader;
            _mapLoader = mapLoader;
        }

        public async Task<IReadOnlyList<GameDataTranslationDto>> LoadAsync()
        {
            var translations = new List<GameDataTranslationDto>();

            foreach ((string fileName, GameDataType dataType) in _fileNames)
            {
                translations.AddRange(await LoadAsync(fileName, dataType));
            }

            return translations;
        }

        private static string ToNostaleRegionKey(RegionLanguageType type)
        {
            switch (type)
            {
                case RegionLanguageType.FR:
                    return "fr";
                case RegionLanguageType.EN:
                    return "uk";
                case RegionLanguageType.DE:
                    return "de";
                case RegionLanguageType.PL:
                    return "pl";
                case RegionLanguageType.IT:
                    return "it";
                case RegionLanguageType.ES:
                    return "es";
                case RegionLanguageType.CZ:
                    return "cz";
                case RegionLanguageType.TR:
                    return "tr";
                default:
                    return "uk";
            }
        }

        public static Encoding GetEncoding(RegionLanguageType key)
        {
            switch (key)
            {
                case RegionLanguageType.EN:
                case RegionLanguageType.FR:
                case RegionLanguageType.ES:
                    return Encoding.GetEncoding(1252);
                case RegionLanguageType.DE:
                case RegionLanguageType.PL:
                case RegionLanguageType.IT:
                case RegionLanguageType.CZ:
                    return Encoding.GetEncoding(1250);
                case RegionLanguageType.TR:
                    return Encoding.GetEncoding(1254);
                default:
                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }
        }

        private async Task<IReadOnlyList<GameDataTranslationDto>> LoadAsync(string fileToParse, GameDataType dataType)
        {
            var translations = new List<GameDataTranslationDto>();
            HashSet<string> _hashSet = dataType switch
            {
                GameDataType.Item => (await _itemLoader.LoadAsync()).Select(s => s.Name).ToHashSet(),
                GameDataType.Card => (await _cardLoader.LoadAsync()).Select(s => s.Name).ToHashSet(),
                GameDataType.NpcMonster => (await _npcLoader.LoadAsync()).Select(s => s.Name).ToHashSet(),
                GameDataType.Skill => (await _skillLoader.LoadAsync()).Select(s => s.Name).ToHashSet(),
                GameDataType.QuestName => (await _questLoader.LoadAsync()).Select(s => s.Name).ToHashSet(),
                GameDataType.Map => (await _mapLoader.LoadAsync()).Select(s => s.Name).ToHashSet(),
                _ => new HashSet<string>()
            };

            foreach (RegionLanguageType lang in Enum.GetValues<RegionLanguageType>())
            {
                if (lang == RegionLanguageType.RU)
                {
                    continue;
                }

                string fileLang = $"{_config.GameLanguagePath}/{string.Format(fileToParse, ToNostaleRegionKey(lang))}";
                using var langFileStream = new StreamReader(fileLang, GetEncoding(lang));
                string line;
                while ((line = await langFileStream.ReadLineAsync()) != null)
                {
                    string[] lineSave = line.Split('\t');
                    if (lineSave.Length <= 1)
                    {
                        continue;
                    }

                    if (!_hashSet.Contains(lineSave[0]))
                    {
                        continue;
                    }

                    translations.Add(new GameDataTranslationDto
                    {
                        DataType = dataType,
                        Language = lang,
                        Key = lineSave[0],
                        Value = lineSave[1]
                    });
                }
            }

            Log.Info($"[RESOURCE_LOADER] Loaded {translations.Count} Game Data translations of {dataType.ToString()}");

            return translations;
        }
    }
}