using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhoenixLib.Caching
{
    public interface ICachedRepository<in TKey, T>
    {
        T Get(TKey id);
        T Get(TKey id, string prefix);

        T GetOrSet(TKey id, Func<T> fetchDelegate);
        T GetOrSet(TKey id, Func<T> fetchDelegate, TimeSpan timeToKeepInCache);

        Task<T> GetOrSetAsync(TKey id, Func<Task<T>> fetchDelegate);
        Task<T> GetOrSetAsync(TKey id, Func<Task<T>> fetchDelegate, TimeSpan timeToKeepInCache);

        IReadOnlyList<T> GetValues(IEnumerable<TKey> keys);
        IReadOnlyList<T> GetValues(IEnumerable<TKey> keys, string prefix);


        void Set(TKey id, T value);
        void Set(TKey id, T value, TimeSpan timeToKeepInCache);
        void Set(TKey id, T value, string prefix);
        void Set(TKey id, T value, string prefix, TimeSpan timeToKeepInCache);

        Task SetAsync(TKey id, Func<Task<T>> fetchDelegate);
        Task SetAsync(TKey id, Func<Task<T>> fetchDelegate, TimeSpan timeToKeepInCache);

        void Remove(TKey id);
    }
}