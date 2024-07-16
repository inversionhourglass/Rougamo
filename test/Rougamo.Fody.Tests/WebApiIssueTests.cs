#if NET6_0
using Xunit;

namespace Rougamo.Fody.Tests
{
    public class WebApiIssueTests() : TestBase("WebApis.dll")
    {
        protected override string RootNamespace => "WebApis";

        [Fact]
        public void Issue75Test()
        {
            var instance = GetInstance("Issue75");
            instance.GetOpenApiDocument();
        }
    }
}
#endif
