using Issues;
using System;
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

        [Fact]
        public async Task Issue16Test()
        {
            var instance = GetIssuesInstance(nameof(Issue16));

            var items = new List<string>();
            await (Task)instance.TestAsync(items);

            var expected = new[] { Issue16.Flag1, Issue16.Flag1, Issue16.Flag2, Issue16.Flag2 };
            Array.Sort(expected);
            items.Sort();
            Assert.Equal(expected, items);
        }

        [Fact]
        public async Task Issue20Test()
        {
            var instance = GetIssuesInstance(nameof(Issue20));
            var expected = new[] { "OnEntry-1", "OnEntry-2", "OnExit-2", "OnExit-1" };

            var items = new List<string>();
            await (Task)instance.M(items);
            Assert.Equal(expected, items);
        }

        private dynamic GetIssuesInstance(string className) => GetInstance($"Issues.{className}");

        private dynamic GetIssuesStaticInstance(string className) => GetStaticInstance($"Issues.{className}");
    }
}
