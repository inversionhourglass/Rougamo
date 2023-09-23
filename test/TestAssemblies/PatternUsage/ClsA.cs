using PatternUsage.Attributes.Properties;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PatternUsage
{
    [InlineDeclare(Pattern = "method(* *(..))")]
    public class ClsA : NonPublicCaller, InterfaceA
    {
        public List<string>? PublicProp { get; set; }

        private static List<string>? StaticPrivateProp { get; set; }

        protected internal Dictionary<string, string>? ProtectedInternalProp { get; set; }

        SortedList<string, string>? DefaultProp { get; set; }

        protected void Protected(List<string> executedMos) { }

        internal static Task<int> StaticInternalAsync(List<string> executedMos) => Task.FromResult(0);

        private protected List<string>? PrivateProtected() => default;
    }
}
