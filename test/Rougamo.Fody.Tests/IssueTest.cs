using Issues;
using Issues.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Rougamo.Fody.Tests
{
    public class IssueTest : TestBase
    {
        public IssueTest()
        {
            WeaveAssembly("Issues.dll");
        }

        [Fact]
        public async Task Issue8Test()
        {
            var instance = GetIssuesInstance(nameof(Issue8));

            await (Task)instance.Command();

            instance.Command1();

            instance.Execute();
            
            await Task.Delay(1000);
        }

        [Fact]
        public async Task Issue9Test()
        {
            var instance = GetIssuesStaticInstance(nameof(Issue9));

            await Issue9.ManualTestAsync();
            await (Task)instance.WeaveTestAsync();

            Assert.Equal(Issue9.Nodestrings, (List<string>)instance.Nodestrings);
        }

        private dynamic GetIssuesInstance(string className) => GetInstance($"Issues.{className}");

        private dynamic GetIssuesStaticInstance(string className) => GetStaticInstance($"Issues.{className}");
    }
}
