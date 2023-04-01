using System;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    internal static class CollectionExtensions
    {
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key)) return false;
            dictionary.Add(key, value);
            return true;
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> more)
        {
            foreach (var item in more)
            {
                dictionary.TryAdd(item.Key, item.Value);
            }
        }

        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> more)
        {
            foreach (var item in more)
            {
                hashSet.Add(item);
            }
        }
    }
}
