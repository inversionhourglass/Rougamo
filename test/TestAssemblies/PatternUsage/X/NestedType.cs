using System.Collections.Generic;
using System.Threading.Tasks;

namespace PatternUsage.X
{
    public class NestedType : NonPublicCaller
    {
        internal void M1(List<string> executedMos) { }

        public class Inner : NonPublicCaller
        {
            public void M2(List<string> executedMos) { }

            private static async Task M3(List<string> executedMos) => await Task.Yield();

            public class Deeply : NonPublicCaller
            {
                protected static ValueTask M4(List<string> executedMos) => new ValueTask();
            }
        }
    }
}
