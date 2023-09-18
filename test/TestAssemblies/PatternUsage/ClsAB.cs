using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PatternUsage
{
    public class ClsAB : NonPublicCaller, InterfaceAB
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

        protected static async void ProtectedStaticDoubleGeneric<T1, T2>(List<string> executedMos)
        {
            executedMos.Add(Convert.ToString(default(T1)));
            await Task.Yield();
            executedMos.Add(Convert.ToString(default(T2)));
        }
    }
}
