using System;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    public static class CommonExtensions
    {
        public static bool StartsWithAny(this string value, params string[] prefixes)
        {
            foreach (var prefix in prefixes)
            {
                if (value.StartsWith(prefix)) return true;
            }

            return false;
        }
    }
}
