#if NETCOREAPP3_1
using BasicUsage.Mos;
using Rougamo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicUsage
{
    [Rougamo<ImplementISyncMoClass>]
    [Rougamo<ImplementIAsyncMoClass>]
    [Rougamo<ImplementISyncMoStruct>]
    [Rougamo<ImplementIAsyncMoStruct>]
    public class DefaultInterfaceMethods
    {
        public void M(List<string> executedMos) { }

        public async Task MAsync(List<string> executedMos) => await Task.Yield();
    }
}
#endif
