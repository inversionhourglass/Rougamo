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
    }
}
