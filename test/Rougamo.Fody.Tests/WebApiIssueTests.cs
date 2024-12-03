#if NET6_0
using Xunit;

namespace Rougamo.Fody.Tests
{
    public class WebApiIssueTests
    {
        private static readonly WeavedAssembly Assembly;

        static WebApiIssueTests()
        {
            Assembly = new("WebApis");
        }

        [Fact]
        public void Issue75Test()
        {
            var instance = Assembly.GetInstance("Issue75");
            instance.GetOpenApiDocument();
        }
    }
}
#endif
