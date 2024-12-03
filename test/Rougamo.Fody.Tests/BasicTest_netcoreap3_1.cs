#if NETCOREAPP3_1
using BasicUsage.Mos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Rougamo.Fody.Tests
{
    partial class BasicTest
    {
        [Fact]
        public async Task DefaultInterfaceMethods()
        {
            var instance = Assembly.GetInstance(nameof(DefaultInterfaceMethods));

            var executedMos = new List<string>();
            string[] expected = [nameof(ImplementISyncMoClass), nameof(ImplementIAsyncMoClass), nameof(ImplementISyncMoStruct), nameof(ImplementIAsyncMoStruct)];

            executedMos.Clear();
            instance.M(executedMos);
            Assert.Equal(expected, executedMos);

            executedMos.Clear();
            await (Task)instance.MAsync(executedMos);
            Assert.Equal(expected, executedMos);
        }
    }
}
#endif
