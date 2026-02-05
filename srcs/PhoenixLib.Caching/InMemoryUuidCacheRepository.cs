using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheManager.Core;

namespace PhoenixLib.Caching
{
    public class InMemoryUuidCacheRepository<T> : IUuidKeyCachedRepository<T>
    {
        private static readonly string Prefix = "repo:" + typeof(T).Name.ToLower();

        private static readonly ICacheManager<T> CacheManager = CacheFactory.Build<T>(Prefix,
            settings => { settings.WithSystemRuntimeCacheHandle(Prefix); });

        public void Set(Guid id, T value)
        {
            Set(id, value, Prefix);
        }

        public void Set(Guid id, T value, TimeSpan timeToKeepInCache)
        {
            Set(id, value, Prefix, timeToKeepInCache);
        }

        public void Set(Guid id, T value, string prefix)
        {
            CacheManager.Put(ToKey(prefix, id), value);
        }

        public void Set(Guid id, T value, string prefix, TimeSpan timeToKeepInCache)
        {
            CacheManager.Put(new CacheItem<T>(ToKey(prefix, id), value, ExpirationMode.Sliding, timeToKeepInCache));
        }

        public async Task SetAsync(Guid id, Func<Task<T>> fetchDelegate) => Set(id, await fetchDelegate.Invoke(), Prefix);

        public async Task SetAsync(Guid id, Func<Task<T>> fetchDelegate, TimeSpan timeToKeepInCache) =>
            CacheManager.Put(new CacheItem<T>(ToKey(id), await fetchDelegate.Invoke(), ExpirationMode.Sliding, timeToKeepInCache));

        public void Remove(Guid id)
        {
            CacheManager.Remove(ToKey(id));
        }

        public T Get(Guid id) => Get(id, Prefix);

        public T Get(Guid id, string prefix) => CacheManager.Get(ToKey(prefix, id));

        public T GetOrSet(Guid id, Func<T> fetchDelegate) => CacheManager.GetOrAdd(ToKey(id), fetchDelegate());

        public T GetOrSet(Guid id, Func<T> fetchDelegate, TimeSpan timeToKeepInCache)
        {
            CacheItem<T> cacheItem = CacheManager.GetCacheItem(ToKey(id));
            if (cacheItem != null)
            {
                return cacheItem.Value;
            }

            Set(id, fetchDelegate(), timeToKeepInCache);
            return CacheManager.Get(ToKey(id));
        }

        public async Task<T> GetOrSetAsync(Guid id, Func<Task<T>> fetchDelegate) => CacheManager.GetOrAdd(ToKey(id), await fetchDelegate.Invoke());

        public async Task<T> GetOrSetAsync(Guid id, Func<Task<T>> fetchDelegate, TimeSpan timeToKeepInCache)
        {
            CacheItem<T> cacheItem = CacheManager.GetCacheItem(ToKey(id));
            if (cacheItem != null)
            {
                return cacheItem.Value;
            }

            await SetAsync(id, fetchDelegate, timeToKeepInCache);
            return CacheManager.Get(ToKey(id));
        }

        public IReadOnlyList<T> GetValues(IEnumerable<Guid> keys) => GetValues(keys, Prefix);

        public IReadOnlyList<T> GetValues(IEnumerable<Guid> keys, string prefix)
        {
            return keys.Select(key => CacheManager.Get(ToKey(prefix, key))).Where(result => !result.Equals(default(T)))
                .ToList();
        }

        private static string ToKey(Guid id) => ToKey(Prefix, id);

        private static string ToKey(string prefix, Guid id) => $"{prefix}:{id.ToString()}";
    }
}