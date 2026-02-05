using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using PhoenixLib.MultiLanguage;
using WingsAPI.Data.GameData;
using WingsEmu.Game._i18n;

namespace Plugin.ResourceLoader
{
    public class InMemoryMultilanguageService : IGameLanguageService
    {
        private static readonly string _dataPrefixString = "language:generic:";

        private readonly IKeyValueCache<string> _cacheClient;
        private readonly IGameDataLanguageService _gameDataLanguageService;
        private readonly IResourceLoader<GenericTranslationDto> _genericTranslationsLoader;

        public InMemoryMultilanguageService(IKeyValueCache<string> cacheClient, IResourceLoader<GenericTranslationDto> genericTranslationsLoader, IGameDataLanguageService gameDataLanguageService)
        {
            _cacheClient = cacheClient;
            _genericTranslationsLoader = genericTranslationsLoader;
            _gameDataLanguageService = gameDataLanguageService;
        }

        public string GetLanguage(string key, RegionLanguageType lang) => _cacheClient.GetOrSet(ToKey(key, lang), key.ToUpperInvariant);

        public string GetLanguageFormat(string key, RegionLanguageType lang, params object[] formatParams)
        {
            string keyString = _cacheClient.GetOrSet(ToKey(key, lang), key.ToUpperInvariant);
            try
            {
                return string.Format(keyString, formatParams);
            }
            catch (Exception e)
            {
                Log.Error($"{key} formatting issue in language {lang}", e);
                return keyString;
            }
        }

        public string GetLanguage<T>(T key, RegionLanguageType lang) where T : Enum
        {
            string tmp = _cacheClient.Get(ToKey(key, lang));
            return tmp ?? key.ToString().ToUpperInvariant();
        }

        public string GetLanguageFormat<T>(T key, RegionLanguageType lang, params object[] formatParams) where T : Enum
        {
            string toFormat = GetLanguage(key, lang);
            try
            {
                return string.Format(toFormat, formatParams);
            }
            catch (Exception e)
            {
                Log.Error($"{key} formatting issue in language {lang}", e);
                return toFormat;
            }
        }

        public string GetLanguage(GameDataType dataType, string dataName, RegionLanguageType lang) => _gameDataLanguageService.GetLanguage(dataType, dataName, lang);

        public Dictionary<string, string> GetDataTranslations(GameDataType dataType, RegionLanguageType lang) => _gameDataLanguageService.GetDataTranslations(dataType, lang);

        public async Task Reload(bool isFullReload = false)
        {
            if (isFullReload)
            {
                await _gameDataLanguageService.Reload();
            }

            Log.Info("[MULTILANGUAGE] Loading...");
            IReadOnlyList<GenericTranslationDto> gameDialogTranslations = await _genericTranslationsLoader.LoadAsync();
            foreach (GenericTranslationDto tmp in gameDialogTranslations)
            {
                _cacheClient.Set(ToKey(tmp.Key, tmp.Language), tmp.Value);
            }

            Log.Info($"[MULTILANGUAGE] loaded {gameDialogTranslations.Count.ToString()} generic translations");
        }

        private static string LangSuffix(RegionLanguageType lang) => lang.ToString().ToLower();
        private static string ToKey(string id, RegionLanguageType lang) => _dataPrefixString + LangSuffix(lang) + ':' + id.ToUpperInvariant();
        private static string ToKey<T>(T id, RegionLanguageType lang) where T : Enum => _dataPrefixString + LangSuffix(lang) + ':' + id.ToString().ToUpperInvariant();
    }
}