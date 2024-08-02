using System;
using System.Collections.Generic;
using System.Linq;

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

        public static List<T> Add<T>(this List<T> list, IEnumerable<T>? items)
        {
            if (items != null)
            {
                list.AddRange(items);
            }

            return list;
        }

        public static T AddGet<T>(this List<T> list, T item)
        {
            list.Add(item);
            return item;
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

        public static IEnumerable<T> UnionIf<T>(this IEnumerable<T> source, bool condition, Func<IEnumerable<T>> func)
        {
            if (!condition) return source;

            return source.Union(func());
        }
    }
}
