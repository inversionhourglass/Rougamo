using Issues;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Rougamo.Fody.Tests
{
    public class IssueTest : TestBase
    {
        public IssueTest() : base("Issues.dll")
        {
        }

        protected override string RootNamespace => "Issues";

        [Fact]
        public async Task Issue8Test()
        {
            var instance = GetInstance(nameof(Issue8));

            await (Task)instance.Command();

            instance.Command1();

            instance.Execute();
            
            await Task.Delay(1000);
        }

        [Fact]
        public async Task Issue9Test()
        {
            var instance = GetStaticInstance(nameof(Issue9));

            await Issue9.ManualTestAsync();
            await (Task)instance.WeaveTestAsync();

            Assert.Equal(Issue9.Nodestrings, (List<string>)instance.Nodestrings);
        }

        [Fact]
        public async Task Issue16Test()
        {
            var instance = GetInstance(nameof(Issue16));

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
            var instance = GetInstance(nameof(Issue20));
            var expected = new[] { "OnEntry-1", "OnEntry-2", "OnExit-2", "OnExit-1" };

            var items = new List<string>();
            await (Task)instance.M(items);
            Assert.Equal(expected, items);
        }

        [Fact]
        public async Task Issue25Test()
        {
            var instance = GetInstance(nameof(Issue25));

            instance.GenericMethod<System.IO.MemoryStream>();
            await (Task)instance.AsyncGenericMethod<System.IO.MemoryStream>();
        }

        [Fact]
        public async Task Issue27Test()
        {
            var instance = GetInstance(nameof(Issue27));

            instance.Get<object>();
            await (Task<object>)instance.GetAsync<object>();
        }
    }
}
