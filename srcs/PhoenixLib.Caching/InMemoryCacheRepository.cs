using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheManager.Core;

namespace PhoenixLib.Caching
{
    public class InMemoryCacheRepository<T> : ILongKeyCachedRepository<T>
    {
        private static readonly string Prefix = "repo:" + typeof(T).Name.ToLower();

        private static readonly ICacheManager<T> CacheManager = CacheFactory.Build<T>(Prefix,
            settings => { settings.WithSystemRuntimeCacheHandle(Prefix); });

        public void Set(long id, T value)
        {
            Set(id, value, Prefix);
        }

        public void Set(long id, T value, TimeSpan timeToKeepInCache)
        {
            Set(id, value, Prefix, timeToKeepInCache);
        }

        public void Set(long id, T value, string prefix)
        {
            CacheManager.Put(ToKey(prefix, id), value);
        }

        public void Set(long id, T value, string prefix, TimeSpan timeToKeepInCache)
        {
            CacheManager.Put(new CacheItem<T>(ToKey(prefix, id), value, ExpirationMode.Sliding, timeToKeepInCache));
        }

        public async Task SetAsync(long id, Func<Task<T>> fetchDelegate) => Set(id, await fetchDelegate.Invoke(), Prefix);

        public async Task SetAsync(long id, Func<Task<T>> fetchDelegate, TimeSpan timeToKeepInCache) =>
            CacheManager.Put(new CacheItem<T>(ToKey(id), await fetchDelegate.Invoke(), ExpirationMode.Sliding, timeToKeepInCache));

        public void Remove(long id)
        {
            CacheManager.Remove(ToKey(id));
        }

        public T Get(long id) => Get(id, Prefix);

        public T Get(long id, string prefix) => CacheManager.Get(ToKey(prefix, id));

        public T GetOrSet(long id, Func<T> fetchDelegate)
        {
            CacheItem<T> cacheItem = CacheManager.GetCacheItem(ToKey(id));
            if (cacheItem != null)
            {
                return cacheItem.Value;
            }

            Set(id, fetchDelegate());
            return CacheManager.Get(ToKey(id));
        }

        public T GetOrSet(long id, Func<T> fetchDelegate, TimeSpan timeToKeepInCache)
        {
            CacheItem<T> cacheItem = CacheManager.GetCacheItem(ToKey(id));
            if (cacheItem != null)
            {
                return cacheItem.Value;
            }

            Set(id, fetchDelegate(), timeToKeepInCache);
            return CacheManager.Get(ToKey(id));
        }

        public async Task<T> GetOrSetAsync(long id, Func<Task<T>> fetchDelegate) => CacheManager.GetOrAdd(ToKey(id), await fetchDelegate.Invoke());

        public async Task<T> GetOrSetAsync(long id, Func<Task<T>> fetchDelegate, TimeSpan timeToKeepInCache)
        {
            CacheItem<T> cacheItem = CacheManager.GetCacheItem(ToKey(id));
            if (cacheItem != null)
            {
                return cacheItem.Value;
            }

            await SetAsync(id, fetchDelegate, timeToKeepInCache);
            return CacheManager.Get(ToKey(id));
        }

        public IReadOnlyList<T> GetValues(IEnumerable<long> keys) => GetValues(keys, Prefix);

        public IReadOnlyList<T> GetValues(IEnumerable<long> keys, string prefix)
        {
            return keys.Select(key => CacheManager.Get(ToKey(prefix, key))).Where(result => !result.Equals(default(T)))
                .ToList();
        }

        private static string ToKey(long id) => ToKey(Prefix, id);

        private static string ToKey(string prefix, long id) => $"{prefix}:{id}";
    }
}