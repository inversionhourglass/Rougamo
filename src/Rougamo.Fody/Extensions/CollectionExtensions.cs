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

        public static void Remove<T>(this List<T> items, Func<T, bool> predicate)
        {
            var stack = new Stack<int>();
            for (var i = 0; i < items.Count; i++)
            {
                if (predicate(items[i]))
                {
                    stack.Push(i);
                }
            }
            while (stack.Count > 0)
            {
                items.RemoveAt(stack.Pop());
            }
        }

        public static void Remove<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Func<TKey, TValue, bool> predicate)
        {
            var list = new List<TKey>();
            foreach (var item in dictionary)
            {
                if(predicate(item.Key, item.Value))
                {
                    list.Add(item.Key);
                }
            }
            foreach (var key in list)
            {
                dictionary.Remove(key);
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
