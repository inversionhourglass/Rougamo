using BasicUsage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rougamo.Fody.Tests
{
    [Collection(nameof(BasicUsage))]
    public class FeatureAiteratorTests : TestBase
    {
        public FeatureAiteratorTests() : base("BasicUsage.dll")
        {
        }

        protected override string RootNamespace => "BasicUsage";

        [Fact]
        public async Task AiteratorFeatureOnEntryTest()
        {
            var instance = GetInstance("FeatureAiteratorUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            var p1 = nameof(AiteratorFeatureOnEntryTest);
            var v1 = await ((IAsyncEnumerable<string>)instance.OnEntry(p1)).ToArrayAsync();
            Assert.Equal(FeatureAiteratorUseCase.Expect_OnEntry, actual);
            Assert.Equal(p1, v1.Last());

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var v2 = await ((IAsyncEnumerable<string>)instance.OnEntry_RewriteArgs(guid1)).ToArrayAsync();
            Assert.Equal(FeatureAiteratorUseCase.Expect_OnEntry, actual);
            Assert.Equal(guid1.ToString(), v2.Last());

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => { await ((IAsyncEnumerable<string>)instance.OnEntry_Exception()).ToArrayAsync(); });
            Assert.Equal(FeatureAiteratorUseCase.Expect_OnEntry, actual);
        }

        [Fact]
        public async Task AiteratorFeatureOnExceptionTest()
        {
            var instance = GetInstance("FeatureAiteratorUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            var p1 = nameof(AiteratorFeatureOnEntryTest);
            var v1 = await ((IAsyncEnumerable<string>)instance.OnException(p1)).ToArrayAsync();
            Assert.Empty(actual);
            Assert.Equal(p1, v1.Last());

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var v2 = await ((IAsyncEnumerable<string>)instance.OnException_RewriteArgs(guid1)).ToArrayAsync();
            Assert.Empty(actual);
            Assert.Equal(guid1.ToString(), v2.Last());

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => { await ((IAsyncEnumerable<string>)instance.OnException_Exception()).ToArrayAsync(); });
            Assert.Equal(FeatureAiteratorUseCase.Expect_OnException, actual);
        }

        [Fact]
        public async Task AiteratorFeatureOnSuccessTest()
        {
            var instance = GetInstance("FeatureAiteratorUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            var p1 = nameof(AiteratorFeatureOnEntryTest);
            var v1 = await ((IAsyncEnumerable<string>)instance.OnSuccess(p1)).ToArrayAsync();
            Assert.Equal(FeatureAiteratorUseCase.Expect_OnSuccess, actual);
            Assert.Equal(p1, v1.Last());

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var v2 = await ((IAsyncEnumerable<string>)instance.OnSuccess_RewriteArgs(guid1)).ToArrayAsync();
            Assert.Equal(FeatureAiteratorUseCase.Expect_OnSuccess, actual);
            Assert.Equal(guid1.ToString(), v2.Last());

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => { await ((IAsyncEnumerable<string>)instance.OnSuccess_Exception()).ToArrayAsync(); });
            Assert.Empty(actual);
        }

        [Fact]
        public async Task AiteratorFeatureOnExitTest()
        {
            var instance = GetInstance("FeatureAiteratorUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            var p1 = nameof(AiteratorFeatureOnEntryTest);
            var v1 = await ((IAsyncEnumerable<string>)instance.OnExit(p1)).ToArrayAsync();
            Assert.Equal(FeatureAiteratorUseCase.Expect_OnExit, actual);
            Assert.Equal(p1, v1.Last());

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var v2 = await ((IAsyncEnumerable<string>)instance.OnExit_RewriteArgs(guid1)).ToArrayAsync();
            Assert.Equal(FeatureAiteratorUseCase.Expect_OnExit, actual);
            Assert.Equal(guid1.ToString(), v2.Last());

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => { await ((IAsyncEnumerable<string>)instance.OnExit_Exception()).ToArrayAsync(); });
            Assert.Equal(FeatureAiteratorUseCase.Expect_OnExit, actual);
        }

        [Fact]
        public async Task AiteratorFeatureRewriteArgsTest()
        {
            var instance = GetInstance("FeatureAiteratorUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            var p1 = nameof(AiteratorFeatureOnEntryTest);
            var v1 = await ((IAsyncEnumerable<string>)instance.RewriteArgs(p1)).ToArrayAsync();
            Assert.Equal(FeatureAiteratorUseCase.Expect_RewriteArgs, actual);
            Assert.Equal(p1, v1.Last());

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var v2 = await ((IAsyncEnumerable<string>)instance.RewriteArgs_RewriteArgs(guid1)).ToArrayAsync();
            Assert.Equal(FeatureAiteratorUseCase.Expect_RewriteArgs, actual);
            Assert.NotEqual(guid1.ToString(), v2.Last());

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => { await ((IAsyncEnumerable<string>)instance.RewriteArgs_Exception()).ToArrayAsync(); });
            Assert.Equal(FeatureAiteratorUseCase.Expect_RewriteArgs, actual);
        }

        [Fact]
        public async Task AiteratorFeatureOnEntryExitTest()
        {
            var instance = GetInstance("FeatureAiteratorUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            var p1 = nameof(AiteratorFeatureOnEntryTest);
            var v1 = await ((IAsyncEnumerable<string>)instance.OnEntryExit(p1)).ToArrayAsync();
            Assert.Equal(FeatureAiteratorUseCase.Expect_OnEntryExit, actual);
            Assert.Equal(p1, v1.Last());

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var v2 = await ((IAsyncEnumerable<string>)instance.OnEntryExit_RewriteArgs(guid1)).ToArrayAsync();
            Assert.Equal(FeatureAiteratorUseCase.Expect_OnEntryExit, actual);
            Assert.Equal(guid1.ToString(), v2.Last());
            actual.Clear();

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => { await ((IAsyncEnumerable<string>)instance.OnEntryExit_Exception()).ToArrayAsync(); });
            Assert.Equal(FeatureAiteratorUseCase.Expect_OnEntryExit, actual);
        }

        [Fact]
        public async Task DifferentFeatureApplyTest()
        {
            var instance = GetInstance("FeatureAiteratorUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => await ((IAsyncEnumerable<string>)instance.DiffApplyVariables()).ToArrayAsync());
            Assert.Equal(FeatureAiteratorUseCase.Expect_DiffApplyVariables, actual);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => await ((IAsyncEnumerable<string>)instance.DiffApplyArray()).ToArrayAsync());
            Assert.Equal(FeatureAiteratorUseCase.Expect_DiffApplyArray, actual);
        }
    }
}
