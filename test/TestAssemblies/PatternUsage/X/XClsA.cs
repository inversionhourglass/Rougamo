using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PatternUsage.X
{
    public class XClsA : NonPublicCaller, InterfaceA
    {
        public List<string>? PublicProp { get; set; }

        private static List<string>? StaticPrivateProp { get; set; }

        protected void Protected(List<string> executedMos) { }

        internal static Task<int> StaticInternalAsync(List<string> executedMos) => Task.FromResult(0);

        public async Task PublicSingleGenericAsync<T>(List<string> executedMos)
        {
            await Task.Yield();
            executedMos.Add(Convert.ToString(default(T)));
        }

        protected static async ValueTask ProtectedStaticDoubleGeneric<T1, T2>(List<string> executedMos)
        {
            executedMos.Add(Convert.ToString(default(T1)));
            await Task.Yield();
            executedMos.Add(Convert.ToString(default(T2)));
        }
    }
}
