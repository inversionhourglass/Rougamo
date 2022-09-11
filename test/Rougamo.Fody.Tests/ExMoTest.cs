using ExUsage;
using ExUsage.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Rougamo.Fody.Tests
{
    public class ExMoTest : TestBase
    {
        public ExMoTest() : base("ExUsage.dll")
        {
        }

        protected override string RootNamespace => "ExUsage";

        [Fact]
        public async Task OrderTest()
        {
            var instance = GetInstance(nameof(ExecutionOrder));
            var expectedSucceed = OrderingAttribute.OrderedSucceedExecutions(false, "1", "2");
            var expectedFailed = OrderingAttribute.OrderedFailedExecutions(false, "1", "2");
            var items = new List<string>();

            instance.Sync(items);
            Assert.Equal(expectedSucceed, items);
            items.Clear();

            await (Task)instance.Async(items);
            Assert.Equal(expectedSucceed, items);
            items.Clear();

            await (Task)instance.NonAsync(items);
            Assert.Equal(expectedSucceed, items);
            items.Clear();

            Assert.Throws<InvalidOperationException>(() => instance.SyncException(items));
            Assert.Equal(expectedFailed, items);
            items.Clear();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await (Task)instance.AsyncException1(items));
            Assert.Equal(expectedFailed, items);
            items.Clear();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await (Task)instance.AsyncException2(items));
            Assert.Equal(expectedFailed, items);
            items.Clear();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await (Task)instance.NonAsyncException1(items));
            Assert.Equal(expectedFailed, items);
            items.Clear();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await (Task)instance.NonAsyncException1(items));
            Assert.Equal(expectedFailed, items);
            items.Clear();
        }

        [Fact]
        public async Task EntryReplaceTest()
        {
            var instance = GetStaticInstance(nameof(EntryReplacement));
            string origin;
            string actual;

            origin = EntryReplacement.Sync(1, 2);
            actual = instance.Sync(1, 2);
            Assert.NotEqual(origin, actual);
            Assert.Equal(nameof(EntryReplacement.Sync), actual);

            origin = await EntryReplacement.Async();
            actual = await (Task<string>)instance.Async();
            Assert.NotEqual(origin, actual);
            Assert.Equal(nameof(EntryReplacement.Async), actual);

            await Assert.ThrowsAsync<InvalidOperationException>(() => EntryReplacement.NonAsync());
            actual = await (Task<string>)instance.NonAsync();
            Assert.Equal(nameof(EntryReplacement.NonAsync), actual);

            var originInt = await EntryReplacement.UnmatchTypeNonAsync();
            var actualInt = await (Task<int>)instance.UnmatchTypeNonAsync();
            Assert.Equal(originInt, actualInt);
        }

        [Fact]
        public async Task ExceptionHandleTest()
        {
            var exceptionHandler = new ExceptionHandler();
            var instance = GetInstance(nameof(ExceptionHandler));

            Assert.Throws<InvalidOperationException>(() => exceptionHandler.Sync(1.2, 3.4));
            Assert.Equal(Math.E, instance.Sync(1.2, 3.4));

            await Assert.ThrowsAsync<InvalidOperationException>(() => exceptionHandler.Async1());
            Assert.Equal(Math.E, await instance.Async1());

            await Assert.ThrowsAsync<InvalidOperationException>(() => exceptionHandler.Async2());
            Assert.Equal(Math.E, await instance.Async2());

            await Assert.ThrowsAsync<InvalidOperationException>(() => exceptionHandler.NonAsync1());
            Assert.Equal(Math.E, await instance.NonAsync1());

            await Assert.ThrowsAsync<InvalidOperationException>(() => exceptionHandler.NonAsync2());
            Assert.Equal(Math.E, await instance.NonAsync2());

            await Assert.ThrowsAsync<InvalidOperationException>(() => exceptionHandler.UnmatchTypeNonAsync1());
            await Assert.ThrowsAsync<InvalidOperationException>(() => (Task<double>)instance.UnmatchTypeNonAsync1());

            await Assert.ThrowsAsync<InvalidOperationException>(() => exceptionHandler.UnmatchTypeNonAsync2());
            await Assert.ThrowsAsync<InvalidOperationException>(() => (Task<float>)instance.UnmatchTypeNonAsync2());
        }

        [Fact]
        public async Task SuccessReplaceTest()
        {
            var instance = GetStaticInstance(nameof(SuccessReplacement));
            int origin;
            int actual;

            origin = SuccessReplacement.Sync();
            actual = instance.Sync();
            Assert.NotEqual(origin, actual);
            Assert.Equal(0, actual);

            origin = await SuccessReplacement.Async();
            actual = await instance.Async();
            Assert.NotEqual(origin, actual);
            Assert.Equal(0, actual);

            origin = await SuccessReplacement.NonAsync();
            actual = await instance.NonAsync();
            Assert.NotEqual(origin, actual);
            Assert.Equal(0, actual);

            var originString = await SuccessReplacement.UnmatchTypeNonAsync();
            var actualString = await (Task<string>)instance.UnmatchTypeNonAsync();
            Assert.Equal(originString, actualString);
        }
    }
}
