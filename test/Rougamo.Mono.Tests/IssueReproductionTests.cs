using Xunit;
using Rougamo.Fody.Tests;

namespace Rougamo.Mono.Tests
{
    public class IssueReproductionTests
    {
        [Fact]
        public void Test_Mono_Runtime_Does_Not_Crash()
        {
            var weavedAssembly = new WeavedAssembly("Issues");
            var instance = weavedAssembly.GetInstance("Issue108`1", false, t => t.MakeGenericType(typeof(string)));
            var result = instance.CatchAndGetStackTrace(108, "mono-stacktrace");
            Assert.True(true);
        }
    }
}
