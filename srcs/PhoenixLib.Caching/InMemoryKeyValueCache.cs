using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheManager.Core;

namespace PhoenixLib.Caching
{
    public class InMemoryKeyValueCache<T> : IKeyValueCache<T>
    {
        private static readonly string Prefix = "kv:" + typeof(T).Name.ToLower();

        private static readonly ICacheManager<T> CacheManager = CacheFactory.Build<T>(Prefix,
            settings => { settings.WithSystemRuntimeCacheHandle(Prefix); });

        public void Set(string id, T value)
        {
            Set(id, value, Prefix);
        }

        public void Set(string id, T value, string prefix)
        {
            CacheManager.Put(ToKey(prefix, id), value);
        }

        public void Set(string id, T value, TimeSpan timeToKeepInCache)
        {
            Set(id, value, Prefix, timeToKeepInCache);
        }

        public void Set(string id, T value, string prefix, TimeSpan timeToKeepInCache)
        {
            CacheManager.Put(new CacheItem<T>(ToKey(prefix, id), value, ExpirationMode.Sliding, timeToKeepInCache));
        }

        public async Task SetAsync(string id, Func<Task<T>> fetchDelegate)
        {
            T obj = await fetchDelegate.Invoke();
            Set(id, obj);
        }

        public async Task SetAsync(string id, Func<Task<T>> fetchDelegate, TimeSpan timeToKeepInCache)
        {
            T obj = await fetchDelegate.Invoke();
            Set(id, obj, timeToKeepInCache);
        }

        public void Remove(string id)
        {
            CacheManager.Remove(ToKey(id));
        }

        public T Get(string id) => Get(id, Prefix);

        public T Get(string id, string prefix) => CacheManager.Get<T>(ToKey(prefix, id));

        public T GetOrSet(string id, Func<T> fetchDelegate)
        {
            CacheItem<T> cacheItem = CacheManager.GetCacheItem(ToKey(id));
            if (cacheItem != null)
            {
                return cacheItem.Value;
            }

            T obj = fetchDelegate();
            Set(id, obj);
            return CacheManager.Get(ToKey(id));
        }

        public T GetOrSet(string id, Func<T> fetchDelegate, TimeSpan timeToKeepInCache)
        {
            CacheItem<T> cacheItem = CacheManager.GetCacheItem(ToKey(id));
            if (cacheItem != null)
            {
                return cacheItem.Value;
            }

            T obj = fetchDelegate();
            Set(id, obj, timeToKeepInCache);
            return CacheManager.Get(ToKey(id));
        }

        public async Task<T> GetOrSetAsync(string id, Func<Task<T>> fetchDelegate) => CacheManager.GetOrAdd(ToKey(id), await fetchDelegate.Invoke());

        public async Task<T> GetOrSetAsync(string id, Func<Task<T>> fetchDelegate, TimeSpan timeToKeepInCache)
        {
            CacheItem<T> cacheItem = CacheManager.GetCacheItem(ToKey(id));
            if (cacheItem != null)
            {
                return cacheItem.Value;
            }

            await SetAsync(id, fetchDelegate, timeToKeepInCache);
            return CacheManager.Get(ToKey(id));
        }

        public IReadOnlyList<T> GetValues(IEnumerable<string> keys) => GetValues(keys, Prefix);

        public IReadOnlyList<T> GetValues(IEnumerable<string> keys, string prefix)
        {
            return keys.Select(key => CacheManager.Get(ToKey(key))).Where(result => !result.Equals(default(T))).ToList();
        }

        private static string ToKey(string prefix, string id) => $"{prefix}:{id}";
        private static string ToKey(string id) => ToKey(Prefix, id);
    }
}