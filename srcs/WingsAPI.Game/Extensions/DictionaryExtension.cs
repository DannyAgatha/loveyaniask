using System.Collections.Generic;

namespace WingsEmu.Core.Extensions;

public static class DictionaryExtension
{
    public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default) =>
        dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;

    public static TValue GetOrSetDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
    {
        if (dictionary.TryGetValue(key, out TValue value))
        {
            return value;
        }

        dictionary[key] = defaultValue;
        return defaultValue;
    }
}