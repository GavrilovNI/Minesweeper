using Sandbox;
using System.Collections.Generic;

namespace Minesweeper.Networking;

public static class NetDictionaryExtensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(this NetDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default)
    {
        if(dictionary.TryGetValue(key, out var value))
            return value;
        return defaultValue!;
    }

    public static IEnumerable<KeyValuePair<TKey, TValue>> ToEnumerable<TKey, TValue>(this NetDictionary<TKey, TValue> dictionary)
    {
        foreach(var item in dictionary)
            yield return item;
    }
}
