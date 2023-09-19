using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PatternUsage.X
{
    public class DoubleGeneric<T1, T2> : NonPublicCaller
    {
        public List<string>? PublicProp { get; set; }

        internal static Task<int> StaticInternalAsync(List<string> executedMos) => Task.FromResult(0);

        protected void ProtectedGeneric<TP>(List<string> executedMos)
        {
            executedMos.Add(Convert.ToString(typeof(T1)));
            executedMos.Add(Convert.ToString(default(TP)));
            executedMos.Add(Convert.ToString(typeof(T2)));
        }
    }
}
