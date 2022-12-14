using System;
using System.Collections.Generic;

namespace Rougamo
{
    internal static class Extensions
    {
        public static void ForEach<T>(this IReadOnlyList<T> items, bool reverse, Action<T> action)
        {
            if (reverse)
            {
                for (var i = items.Count - 1; i >= 0; i--)
                {
                    action(items[i]);
                }
            }
            else
            {
                for (var i = 0; i < items.Count; i++)
                {
                    action(items[i]);
                }
            }
        }
    }
}
