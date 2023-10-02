using PatternUsage.Attributes.Methods;
using System.Collections.Generic;

namespace PatternUsage
{
    [GenericParameterMatch1]
    [GenericParameterMatch2]
    public class TheGeneric<T1, T2> : NonPublicCaller
    {
        public List<string>? M1<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            return null;
        }

        public List<string>? M2<T3, T4>(T2 t2, T1 t1, T4 t4, T3 t3)
        {
            return null;
        }
    }
}
