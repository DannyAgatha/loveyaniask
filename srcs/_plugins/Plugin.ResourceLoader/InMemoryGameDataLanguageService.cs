using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using PhoenixLib.MultiLanguage;
using WingsAPI.Data.GameData;
using WingsEmu.Game._i18n;

namespace Plugin.ResourceLoader
{
    public class InMemoryGameDataLanguageService : IGameDataLanguageService
    {
        private static readonly string _dataPrefixGameData = "language:game-data:";
        private readonly IKeyValueCache<string> _cacheClient;
        private readonly IResourceLoader<GameDataTranslationDto> _dataTranslationsLoader;

        private Dictionary<(GameDataType DataType, RegionLanguageType Language), Dictionary<string, string>> _gameDataLanguages = new();

        public InMemoryGameDataLanguageService(IKeyValueCache<string> cacheClient, IResourceLoader<GameDataTranslationDto> dataTranslationsLoader)
        {
            _cacheClient = cacheClient;
            _dataTranslationsLoader = dataTranslationsLoader;
        }


        public string GetLanguage(GameDataType dataType, string dataName, RegionLanguageType lang)
        {
            return _cacheClient.GetOrSet(ToKey(dataType, dataName, lang), () => dataName);
        }

        public Dictionary<string, string> GetDataTranslations(GameDataType dataType, RegionLanguageType lang) => _gameDataLanguages[(dataType, lang)];

        public async Task Reload(bool isFullReload = false)
        {
            Log.Info("[MULTILANGUAGE] Loading...");
            IReadOnlyList<GameDataTranslationDto> gameDataTranslations = await _dataTranslationsLoader.LoadAsync();
            foreach (GameDataTranslationDto tmp in gameDataTranslations)
            {
                _cacheClient.Set(ToKey(tmp.DataType, tmp.Key, tmp.Language), tmp.Value);
            }

            var newDictionary = gameDataTranslations.GroupBy(s => (s.DataType, s.Language)).ToDictionary(s => s.Key, s => s.ToDictionary(p => p.Key, p => p.Value));

            Interlocked.Exchange(ref _gameDataLanguages, newDictionary);

            Log.Info($"[MULTILANGUAGE] loaded {gameDataTranslations.Count.ToString()} game data translations");
        }

        private static string LangSuffix(RegionLanguageType lang) => lang.ToString().ToLower();
        private static string GameDataSuffix(GameDataType dataType) => dataType.ToString().ToLower();
        private static string ToKey(GameDataType dataType, string dataName, RegionLanguageType lang) => _dataPrefixGameData + LangSuffix(lang) + ':' + GameDataSuffix(dataType) + ':' + dataName;
    }
}