// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundatio.Caching;
using Foundatio.Serializer;
using StackExchange.Redis;

namespace PhoenixLib.MultiLanguage
{
    public class GenericMultilanguageService<T> : IEnumBasedLanguageService<T> where T : struct, Enum
    {
        private readonly ICacheClient _cacheClient;
        private readonly string _dataPrefixString;
        private readonly string _keySetKeyString;

        public GenericMultilanguageService(IConnectionMultiplexer conf)
        {
            _dataPrefixString = $"data:{typeof(T).Name.ToLower()}:";
            _keySetKeyString = $"keys:{typeof(T).Name.ToLower()}:";
            _cacheClient = new RedisCacheClient(new RedisCacheClientOptions
            {
                ConnectionMultiplexer = conf,
                Serializer = new JsonNetSerializer()
            });
        }

        public string GetLanguage(T key, RegionLanguageType lang) => GetLanguageAsync(key, lang).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task<string> GetLanguageAsync(T key, RegionLanguageType lang)
        {
            CacheValue<string> value = await _cacheClient.GetAsync<string>(ToKey(key, lang));
            if (value.HasValue)
            {
                return value.Value;
            }

            string newLanguageValue = key.ToString().ToUpper();
            await SetLanguageAsync(key, newLanguageValue, lang);
            return newLanguageValue;
        }

        public async Task<IDictionary<T, string>> GetLanguageAsync(ICollection<T> key, RegionLanguageType type)
        {
            CacheValue<ICollection<string>> set = await _cacheClient.GetListAsync<string>(KeySetKeyString(type)).ConfigureAwait(false);
            IDictionary<string, CacheValue<string>> dico = await _cacheClient.GetAllAsync<string>(set.Value).ConfigureAwait(false);
            return dico.ToDictionary(s => Enum.Parse<T>(s.Key.Substring(s.Key.LastIndexOf(':') + 1)), s => s.Value.Value);
        }

        public void SetLanguage(T key, string value, RegionLanguageType lang)
        {
            SetLanguageAsync(key, value, lang).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task SetLanguageAsync(T key, string value, RegionLanguageType lang)
        {
            await RegisterGameDialogKeyAsync(new[] { key }, lang).ConfigureAwait(false);
            await _cacheClient.SetAsync(ToKey(key, lang), value).ConfigureAwait(false);
        }

        public async Task SetLanguageAsync(IDictionary<T, string> keyValues, RegionLanguageType type)
        {
            var dico = keyValues.ToDictionary(s => ToKey(s.Key, type), s => s.Value);
            await RegisterGameDialogKeyAsync(keyValues.Keys.ToArray(), type);
            await _cacheClient.SetAllAsync(dico);
        }

        private static string LangSuffix(RegionLanguageType lang) => lang.ToString().ToLower();
        private string KeySetKeyString(RegionLanguageType lang) => _keySetKeyString + LangSuffix(lang);
        private string ToKey(T id, RegionLanguageType lang) => _dataPrefixString + LangSuffix(lang) + ':' + id.ToString().ToUpper();
        private async Task<CacheValue<ICollection<string>>> KeySet(RegionLanguageType lang) => await _cacheClient.GetListAsync<string>(KeySetKeyString(lang));
        private async Task<ICollection<string>> GetAllStringKeysAsync(RegionLanguageType lang) => (await KeySet(lang).ConfigureAwait(false)).Value;

        private async Task RegisterGameDialogKeyAsync(IEnumerable<T> keys, RegionLanguageType lang)
        {
            await _cacheClient.ListAddAsync(_keySetKeyString + lang.ToString().ToLower(), keys.Select(s => ToKey(s, lang)));
        }

        public void SetLanguage(Dictionary<T, string> keyValues, RegionLanguageType lang)
        {
            SetLanguageAsync(keyValues, lang).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task SetLanguageAsync(Dictionary<T, string> keyValues, RegionLanguageType lang)
        {
            var dico = keyValues.ToDictionary(s => ToKey(s.Key, lang), s => s.Value);
            await RegisterGameDialogKeyAsync(keyValues.Keys.ToArray(), lang);
            await _cacheClient.SetAllAsync(dico);
        }


        public Dictionary<string, string> GetLanguages(RegionLanguageType lang)
        {
            CacheValue<ICollection<string>> set = _cacheClient.GetListAsync<string>(KeySetKeyString(lang)).ConfigureAwait(false).GetAwaiter().GetResult();
            IDictionary<string, CacheValue<string>> dico = _cacheClient.GetAllAsync<string>(set.Value).ConfigureAwait(false).GetAwaiter().GetResult();

            return dico.ToDictionary(s => s.Key, s => s.Value.Value);
        }
    }
}