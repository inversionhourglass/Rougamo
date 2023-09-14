using PatternUsage.Attributes.Methods;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PatternUsage
{
    [InstaceSuffix]
    public class Public : NonPublicCaller, Interface
    {
        public void Instance(List<string> executedMos)
        {
        }

        private string? PrivateInstance(List<string> executedMos) => default;

        protected int ProtectedInstance(List<string> executedMos) => default;

        internal double InternalInstanceXyz(List<string> executedMos) => default;

        private protected Task P2InstanceAsync(List<string> executedMos) => Task.CompletedTask;

        protected internal async ValueTask<int> PiInstanceAsync(List<string> executedMos)
        {
            await Task.Yield();
            return 1;
        }

        public static async Task StaticAsync(List<string> executedMos)
        {
            await Task.Yield();
        }

        private static async ValueTask PrivateStaticAsync(List<string> executedMos)
        {
            await Task.Yield();
        }
    }
}
