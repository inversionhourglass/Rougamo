using System;

namespace Rougamo.Fody
{
    internal static class CommonExtensions
    {
        public static bool HasIntersection<T>(this T set1, int set2) where T : Enum
        {
            var iSet1 = Convert.ToInt32(set1);
            return (iSet1 & set2) != 0;
        }

        public static bool Contains<T>(this T set1, int set2) where T : Enum
        {
            var iSet1 = Convert.ToInt32(set1);
            return (iSet1 & set2) == set2;
        }

        public static bool HasIntersection<T>(this T set1, T set2) where T : Enum
        {
            var iSet2 = Convert.ToInt32(set2);
            return HasIntersection(set1, iSet2);
        }

        public static bool Contains<T>(this T set1, T set2) where T : Enum
        {
            var iSet2 = Convert.ToInt32(set2);
            return Contains(set1, iSet2);
        }
    }
}
