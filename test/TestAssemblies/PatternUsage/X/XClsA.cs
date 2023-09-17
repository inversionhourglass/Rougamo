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
    }
}
