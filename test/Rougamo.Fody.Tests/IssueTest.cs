using Issues;
using Issues.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rougamo.Fody.Tests
{
    public class IssueTest
    {
        private static readonly WeavedAssembly Assembly;

        static IssueTest()
        {
            Assembly = new("Issues");
        }

        [Fact]
        public async Task Issue8Test()
        {
            var instance = Assembly.GetInstance(nameof(Issue8));

            await (Task)instance.Command();

            instance.Command1();

            instance.Execute();

            await Task.Delay(1000);
        }

        [Fact]
        public async Task Issue9Test()
        {
            var instance = Assembly.GetStaticInstance(nameof(Issue9));

            await Issue9.ManualTestAsync();
            await (Task)instance.WeaveTestAsync();

            Assert.Equal(Issue9.Nodestrings, (List<string>)instance.Nodestrings);
        }

        [Fact]
        public async Task Issue16Test()
        {
            var instance = Assembly.GetInstance(nameof(Issue16));

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
            var instance = Assembly.GetInstance(nameof(Issue20));
            var expected = new[] { "OnEntry-1", "OnEntry-2", "OnExit-2", "OnExit-1" };

            var items = new List<string>();
            await (Task)instance.M(items);
            Assert.Equal(expected, items);
        }

        [Fact]
        public async Task Issue25Test()
        {
            var instance = Assembly.GetInstance(nameof(Issue25));

            instance.GenericMethod<System.IO.MemoryStream>();
            await (Task)instance.AsyncGenericMethod<System.IO.MemoryStream>();
        }

        [Fact]
        public async Task Issue27Test()
        {
            var instance = Assembly.GetInstance(nameof(Issue27));

            instance.Get<object>();
            await (Task<object>)instance.GetAsync<object>();
        }

#if !DEBUG
        // This issue only appears in release mode
        [Fact]
        public async Task Issue29Test()
        {
            var instancee = Assembly.GetInstance(nameof(Issue29));

            await (Task)instancee.Test(null);
        }
#endif

        [Fact]
        public async Task Issue30Test()
        {
            var sInstance = Assembly.GetStaticInstance(nameof(Issue30));

            var v = await (Task<string>)sInstance.Test(" 123");
            Assert.Equal("123", v);
        }

        [Fact]
        public void Issue40Test()
        {
            var instance = Assembly.GetInstance(nameof(Issue40), false);
            var sInstance = Assembly.GetStaticInstance(nameof(Issue40), false);

            var instanceValue = (int)instance.Instance();
            var staticValue = (int)sInstance.Static();

            Assert.Equal(_40_Attribute.ReplacedValue, instanceValue);
            Assert.Equal(Issue40.Static(), staticValue);
        }

        [Fact]
        public async Task Issue51Test()
        {
            var sInstance = Assembly.GetStaticInstance(nameof(Issue51), false);

            await (Task)sInstance.TestAsync();
        }

        [Fact]
        public async Task Issue60Test()
        {
            var instance = Assembly.GetInstance(nameof(Issue60), false);

            var items = new List<string>();
            await (ValueTask)instance.Async(items);
            Assert.Equal(_60_Attribute.SUCCEED, items);

            items.Clear();
            await Assert.ThrowsAsync<Exception>(async () => await (ValueTask)instance.AsyncException(items));
            Assert.Equal(_60_Attribute.FAILED, items);
        }

        [Fact]
        public void Issue61Test()
        {
            object instance = Assembly.GetInstance(nameof(Issue61), false);

            Issue61.In mReadOnlySpanIn = Assembly.GetMethodDelegate<Issue61.In>(instance, nameof(Issue61.ReadOnlySpanIn));
            Issue61.Out mReadOnlySpanOut = Assembly.GetMethodDelegate<Issue61.Out>(instance, nameof(Issue61.ReadOnlySpanOut));

            var pIn = new ReadOnlySpan<char>("abc".ToCharArray());
            var pOut = "xyz";
            var str = mReadOnlySpanIn(pIn);
            var span = mReadOnlySpanOut(pOut);
            Assert.Equal(pIn.ToString(), str);
            Assert.Equal(pOut, span.ToString());
        }

        [Fact]
        public async void Issue63Test()
        {
            var instance = Assembly.GetInstance(nameof(Issue63));
            var sInstance = Assembly.GetStaticInstance(nameof(Issue63));

            var iIn = 1;
            var iOut = instance.WillBeReplaced(iIn);
            Assert.NotEqual(iIn, iOut);

            var fIn = 1f;
            var fOut = await (ValueTask<float>)instance.WontBeReplaced(fIn);
            Assert.Equal(fIn, fOut);

            var sIn = string.Empty;
            var sOut = sInstance.WillBeReplaced(sIn);
            Assert.NotEqual(sIn, sOut);
        }

        [Fact]
        public void Issue66Test()
        {
            var instance = Assembly.GetInstance("Issue66`2", false, t => t.MakeGenericType(typeof(int), typeof(int)));
            instance.M();
        }

        [Fact]
        public void Issue72Test()
        {
            var instance = Assembly.GetInstance(nameof(Issue72));
            var logs = new List<string>();
            instance.Plus(logs, 1, 2);

            Assert.Equal(["OnEntry"], logs);
        }

        [Fact]
        public async Task Issue73Test()
        {
            var instance = Assembly.GetInstance("Issue73", false, t =>
            {
                var nt = t.GetNestedTypes().Single(x => x.Name == "Cls`1");
                return nt.MakeGenericType(typeof(int));
            });

            await (Task)instance.MAsync();
        }

        [Fact]
        public void Issue79Test()
        {
            var instance = Assembly.GetInstance(nameof(Issue79));

            instance.M();
        }

        [Fact]
        public void Issue80Test()
        {
            var instance = Assembly.GetInstance(nameof(Issue80));

            instance.M();
        }

        [Fact]
        public void Issue81Test()
        {
            var instance = Assembly.GetInstance(nameof(Issue81));

            instance.M();
        }

        [Fact]
        public void Issue83Test()
        {
            var instance = Assembly.GetInstance(nameof(Issue83));

            Assert.Throws<Exception>(() => instance.M());
        }

        [Fact]
        public void Issue86Test()
        {
            var instance = Assembly.GetInstance(nameof(Issue86));

            instance.M();
        }

        [Fact]
        public void Issue97Test()
        {
            var instance = Assembly.GetInstance(nameof(Issue97));

            var orders = new List<double>();
            instance.M(orders);

            Assert.Equal(2, orders.Count);
            Assert.Equal([1.0, 2.0], orders);
        }
    }
}
