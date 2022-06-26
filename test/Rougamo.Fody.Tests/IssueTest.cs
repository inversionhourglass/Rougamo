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
            var instance = GetIssuesInstance(nameof(Issues.Issue8));

            await (Task)instance.Command();

            instance.Command1();

            instance.Execute();
            
            await Task.Delay(1000);
        }

        private dynamic GetIssuesInstance(string className) => GetInstance($"Issues.{className}");
    }
}
