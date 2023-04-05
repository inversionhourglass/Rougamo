using BasicUsage;
using BasicUsage.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Rougamo.Fody.Tests
{
    [Collection(nameof(BasicUsage))]
    public class FeatureAsyncTests : TestBase
    {
        public FeatureAsyncTests() : base("BasicUsage.dll")
        {
        }

        protected override string RootNamespace => "BasicUsage";

        [Fact]
        public async Task AsyncFeatureOnEntryTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await (Task<string>)instance.OnEntry(string.Empty);
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntry, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = await (ValueTask<Guid>)instance.OnEntry_RewriteArgs(guid1);
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntry, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = await (Task<string>)instance.OnEntry_EntryReplace(a, b);
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntry, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => await (ValueTask)instance.OnEntry_Exception());
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntry, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.RetryException.GetType(), () => (Task<string>)instance.OnEntry_ExceptionRetry());
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntry, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.HandleableException.GetType(), async () => await (ValueTask<string>)instance.OnEntry_ExceptionHandled());
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntry, actual);

            actual.Clear();
            var v5 = await (Task<string>)instance.OnEntry_SuccessRetry();
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntry, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = await (ValueTask<string>)instance.OnEntry_SuccessReplaced();
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntry, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public async Task AsyncFeatureOnExceptionTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await (Task<string>)instance.OnException(string.Empty);
            Assert.Empty(actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = await (ValueTask<Guid>)instance.OnException_RewriteArgs(guid1);
            Assert.Empty(actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = await (Task<string>)instance.OnException_EntryReplace(a, b);
            Assert.Empty(actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => await (ValueTask)instance.OnException_Exception());
            Assert.Equal(FeatureAsyncUseCase.Expect_OnException, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.RetryException.GetType(), () => (Task<string>)instance.OnException_ExceptionRetry());
            Assert.Equal(FeatureAsyncUseCase.Expect_OnException, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.HandleableException.GetType(), async () => await (ValueTask<string>)instance.OnException_ExceptionHandled());
            Assert.Equal(FeatureAsyncUseCase.Expect_OnException, actual);

            actual.Clear();
            var v5 = await (Task<string>)instance.OnException_SuccessRetry();
            Assert.Empty(actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = await (ValueTask<string>)instance.OnException_SuccessReplaced();
            Assert.Empty(actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public async Task AsyncFeatureOnSuccessTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await (Task<string>)instance.OnSuccess(string.Empty);
            Assert.Equal(FeatureAsyncUseCase.Expect_OnSuccess, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = await (ValueTask<Guid>)instance.OnSuccess_RewriteArgs(guid1);
            Assert.Equal(FeatureAsyncUseCase.Expect_OnSuccess, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = await (Task<string>)instance.OnSuccess_EntryReplace(a, b);
            Assert.Equal(FeatureAsyncUseCase.Expect_OnSuccess, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => await (ValueTask)instance.OnSuccess_Exception());
            Assert.Empty(actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.RetryException.GetType(), () => (Task<string>)instance.OnSuccess_ExceptionRetry());
            Assert.Empty(actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.HandleableException.GetType(), async () => await (ValueTask<string>)instance.OnSuccess_ExceptionHandled());
            Assert.Empty(actual);

            actual.Clear();
            var v5 = await (Task<string>)instance.OnSuccess_SuccessRetry();
            Assert.Equal(FeatureAsyncUseCase.Expect_OnSuccess, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = await (ValueTask<string>)instance.OnSuccess_SuccessReplaced();
            Assert.Equal(FeatureAsyncUseCase.Expect_OnSuccess, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public async Task AsyncFeatureOnExitTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await (Task<string>)instance.OnExit(string.Empty);
            Assert.Equal(FeatureAsyncUseCase.Expect_OnExit, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = await (ValueTask<Guid>)instance.OnExit_RewriteArgs(guid1);
            Assert.Equal(FeatureAsyncUseCase.Expect_OnExit, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = await (Task<string>)instance.OnExit_EntryReplace(a, b);
            Assert.Equal(FeatureAsyncUseCase.Expect_OnExit, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => await (ValueTask)instance.OnExit_Exception());
            Assert.Equal(FeatureAsyncUseCase.Expect_OnExit, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.RetryException.GetType(), () => (Task<string>)instance.OnExit_ExceptionRetry());
            Assert.Equal(FeatureAsyncUseCase.Expect_OnExit, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.HandleableException.GetType(), async () => await (ValueTask<string>)instance.OnExit_ExceptionHandled());
            Assert.Equal(FeatureAsyncUseCase.Expect_OnExit, actual);

            actual.Clear();
            var v5 = await (Task<string>)instance.OnExit_SuccessRetry();
            Assert.Equal(FeatureAsyncUseCase.Expect_OnExit, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = await (ValueTask<string>)instance.OnExit_SuccessReplaced();
            Assert.Equal(FeatureAsyncUseCase.Expect_OnExit, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public async Task AsyncFeatureRewriteArgsTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await (Task<string>)instance.RewriteArgs(string.Empty);
            Assert.Equal(FeatureAsyncUseCase.Expect_RewriteArgs, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = await (ValueTask<Guid>)instance.RewriteArgs_RewriteArgs(guid1);
            Assert.Equal(FeatureAsyncUseCase.Expect_RewriteArgs, actual);
            Assert.NotEqual(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = await (Task<string>)instance.RewriteArgs_EntryReplace(a, b);
            Assert.Equal(FeatureAsyncUseCase.Expect_RewriteArgs, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => await (ValueTask)instance.RewriteArgs_Exception());
            Assert.Equal(FeatureAsyncUseCase.Expect_RewriteArgs, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.RetryException.GetType(), () => (Task<string>)instance.RewriteArgs_ExceptionRetry());
            Assert.Equal(FeatureAsyncUseCase.Expect_RewriteArgs, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.HandleableException.GetType(), async () => await (ValueTask<string>)instance.RewriteArgs_ExceptionHandled());
            Assert.Equal(FeatureAsyncUseCase.Expect_RewriteArgs, actual);

            actual.Clear();
            var v5 = await (Task<string>)instance.RewriteArgs_SuccessRetry();
            Assert.Equal(FeatureAsyncUseCase.Expect_RewriteArgs, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = await (ValueTask<string>)instance.RewriteArgs_SuccessReplaced();
            Assert.Equal(FeatureAsyncUseCase.Expect_RewriteArgs, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public async Task AsyncFeatureEntryReplaceTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await (Task<string>)instance.EntryReplace(string.Empty);
            Assert.Equal(FeatureAsyncUseCase.Expect_EntryReplace, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = await (ValueTask<Guid>)instance.EntryReplace_RewriteArgs(guid1);
            Assert.Equal(FeatureAsyncUseCase.Expect_EntryReplace, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = await (Task<string>)instance.EntryReplace_EntryReplace(a, b);
            Assert.Equal(FeatureAsyncUseCase.Expect_EntryReplace, actual);
            Assert.NotEqual(a + b, value);
            Assert.Equal(FeatureAttribute.ReplacedReturn, value);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => await (ValueTask)instance.EntryReplace_Exception());
            Assert.Equal(FeatureAsyncUseCase.Expect_EntryReplace, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.RetryException.GetType(), () => (Task<string>)instance.EntryReplace_ExceptionRetry());
            Assert.Equal(FeatureAsyncUseCase.Expect_EntryReplace, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.HandleableException.GetType(), async () => await (ValueTask<string>)instance.EntryReplace_ExceptionHandled());
            Assert.Equal(FeatureAsyncUseCase.Expect_EntryReplace, actual);

            actual.Clear();
            var v5 = await (Task<string>)instance.EntryReplace_SuccessRetry();
            Assert.Equal(FeatureAsyncUseCase.Expect_EntryReplace, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = await (ValueTask<string>)instance.EntryReplace_SuccessReplaced();
            Assert.Equal(FeatureAsyncUseCase.Expect_EntryReplace, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public async Task AsyncFeatureExceptionHandleTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await (ValueTask<string>)instance.ExceptionHandle(string.Empty);
            Assert.Empty(actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = await (Task<Guid>)instance.ExceptionHandle_RewriteArgs(guid1);
            Assert.Empty(actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = await (ValueTask<string>)instance.ExceptionHandle_EntryReplace(a, b);
            Assert.Empty(actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(() => (Task)instance.ExceptionHandle_Exception());
            Assert.Equal(FeatureAsyncUseCase.Expect_ExceptionHandle, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.RetryException.GetType(), async () => await (ValueTask<string>)instance.ExceptionHandle_ExceptionRetry());
            Assert.Equal(FeatureAsyncUseCase.Expect_ExceptionHandle, actual);

            actual.Clear();
            var v2 = await (Task<string>)instance.ExceptionHandle_ExceptionHandled();
            Assert.Equal(FeatureAsyncUseCase.Expect_ExceptionHandle, actual);
            Assert.Equal(FeatureAttribute.ReplacedReturn, v2);

            actual.Clear();
            var v5 = await (ValueTask<string>)instance.ExceptionHandle_SuccessRetry();
            Assert.Empty(actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = await (Task<string>)instance.ExceptionHandle_SuccessReplaced();
            Assert.Empty(actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public async Task AsyncFeatureExceptionRetryTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await (ValueTask<string>)instance.ExceptionRetry(string.Empty);
            Assert.Empty(actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = await (Task<Guid>)instance.ExceptionRetry_RewriteArgs(guid1);
            Assert.Empty(actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = await (ValueTask<string>)instance.ExceptionRetry_EntryReplace(a, b);
            Assert.Empty(actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(() => (Task)instance.ExceptionRetry_Exception());
            Assert.Equal(FeatureAsyncUseCase.Expect_ExceptionRetry, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.RetryException.GetType(), async () => await (ValueTask<string>)instance.ExceptionRetry_ExceptionRetry());
            Assert.Equal(FeatureAsyncUseCase.Expect_ExceptionRetried, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.HandleableException.GetType(), () => (Task<string>)instance.ExceptionRetry_ExceptionHandled());
            Assert.Equal(FeatureAsyncUseCase.Expect_ExceptionRetry, actual);

            actual.Clear();
            var v5 = await (ValueTask<string>)instance.ExceptionRetry_SuccessRetry();
            Assert.Empty(actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = await (Task<string>)instance.ExceptionRetry_SuccessReplaced();
            Assert.Empty(actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public async Task AsyncFeatureSuccessReplaceTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await (ValueTask<string>)instance.SuccessReplace(string.Empty);
            Assert.Equal(FeatureAsyncUseCase.Expect_SuccessReplace, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = await (Task<Guid>)instance.SuccessReplace_RewriteArgs(guid1);
            Assert.Equal(FeatureAsyncUseCase.Expect_SuccessReplace, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = await (ValueTask<string>)instance.SuccessReplace_EntryReplace(a, b);
            Assert.Equal(FeatureAsyncUseCase.Expect_SuccessReplace, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(() => (Task)instance.SuccessReplace_Exception());
            Assert.Empty(actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.RetryException.GetType(), async () => await (ValueTask<string>)instance.SuccessReplace_ExceptionRetry());
            Assert.Empty(actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.HandleableException.GetType(), () => (Task<string>)instance.SuccessReplace_ExceptionHandled());
            Assert.Empty(actual);

            actual.Clear();
            var v5 = await (ValueTask<string>)instance.SuccessReplace_SuccessRetry();
            Assert.Equal(FeatureAsyncUseCase.Expect_SuccessReplace, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = await (Task<string>)instance.SuccessReplace_SuccessReplaced();
            Assert.Equal(FeatureAsyncUseCase.Expect_SuccessReplace, actual);
            Assert.Equal(FeatureAttribute.ReplacedReturn, v4);
        }

        [Fact]
        public async Task AsyncFeatureSuccessRetryTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await (ValueTask<string>)instance.SuccessRetry(string.Empty);
            Assert.Equal(FeatureAsyncUseCase.Expect_SuccessRetry, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = await (Task<Guid>)instance.SuccessRetry_RewriteArgs(guid1);
            Assert.Equal(FeatureAsyncUseCase.Expect_SuccessRetry, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = await (ValueTask<string>)instance.SuccessRetry_EntryReplace(a, b);
            Assert.Equal(FeatureAsyncUseCase.Expect_SuccessRetry, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(() => (Task)instance.SuccessRetry_Exception());
            Assert.Empty(actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.RetryException.GetType(), async () => await (ValueTask<string>)instance.SuccessRetry_ExceptionRetry());
            Assert.Empty(actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.HandleableException.GetType(), () => (Task<string>)instance.SuccessRetry_ExceptionHandled());
            Assert.Empty(actual);

            actual.Clear();
            var v5 = await (ValueTask<string>)instance.SuccessRetry_SuccessRetry();
            Assert.Equal(FeatureAsyncUseCase.Expect_SuccessRetried, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = await (Task<string>)instance.SuccessRetry_SuccessReplaced();
            Assert.Equal(FeatureAsyncUseCase.Expect_SuccessRetry, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public async Task AsyncFeatureObserveTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await (ValueTask<string>)instance.Observe(string.Empty);
            Assert.Equal(FeatureAsyncUseCase.Expect_Observe, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = await (Task<Guid>)instance.Observe_RewriteArgs(guid1);
            Assert.Equal(FeatureAsyncUseCase.Expect_Observe, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = await (ValueTask<string>)instance.Observe_EntryReplace(a, b);
            Assert.Equal(FeatureAsyncUseCase.Expect_Observe, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(() => (Task)instance.Observe_Exception());
            Assert.Equal(FeatureAsyncUseCase.Expect_Observe_Failed, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.RetryException.GetType(), async () => await (ValueTask<string>)instance.Observe_ExceptionRetry());
            Assert.Equal(FeatureAsyncUseCase.Expect_Observe_Failed, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.HandleableException.GetType(), () => (Task<string>)instance.Observe_ExceptionHandled());
            Assert.Equal(FeatureAsyncUseCase.Expect_Observe_Failed, actual);

            actual.Clear();
            var v5 = await (ValueTask<string>)instance.Observe_SuccessRetry();
            Assert.Equal(FeatureAsyncUseCase.Expect_Observe, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = await (Task<string>)instance.Observe_SuccessReplaced();
            Assert.Equal(FeatureAsyncUseCase.Expect_Observe, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public async Task AsyncFeatureNonRewriteArgsTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await (ValueTask<string>)instance.NonRewriteArgs(string.Empty);
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRewriteArgs, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = await (Task<Guid>)instance.NonRewriteArgs_RewriteArgs(guid1);
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRewriteArgs, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = await (ValueTask<string>)instance.NonRewriteArgs_EntryReplace(a, b);
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRewriteArgs_EntryReplaced, actual);
            Assert.NotEqual(a + b, value);
            Assert.Equal(FeatureAttribute.ReplacedReturn, value);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(() => (Task)instance.NonRewriteArgs_Exception());
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRewriteArgs_Failed, actual);

            actual.Clear();
            var v6 = await (ValueTask<string>)instance.NonRewriteArgs_ExceptionRetry();
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRewriteArgs_ExceptionRetried, actual);
            Assert.Equal(FeatureAttribute.RetriedReturn, v6);

            actual.Clear();
            var v2 = await (Task<string>)instance.NonRewriteArgs_ExceptionHandled();
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRewriteArgs_Failed, actual);
            Assert.Equal(FeatureAttribute.ReplacedReturn, v2);

            actual.Clear();
            var v5 = await (ValueTask<string>)instance.NonRewriteArgs_SuccessRetry();
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRewriteArgs_SuccessRetried, actual);
            Assert.Equal(FeatureAttribute.RetriedReturn, v5);

            actual.Clear();
            var v4 = await (Task<string>)instance.NonRewriteArgs_SuccessReplaced();
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRewriteArgs, actual);
            Assert.Equal(FeatureAttribute.ReplacedReturn, v4);
        }

        [Fact]
        public async Task AsyncFeatureNonRetryTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await (ValueTask<string>)instance.NonRetry(string.Empty);
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRetry, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = await (Task<Guid>)instance.NonRetry_RewriteArgs(guid1);
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRetry, actual);
            Assert.NotEqual(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = await (ValueTask<string>)instance.NonRetry_EntryReplace(a, b);
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRetry_EntryReplaced, actual);
            Assert.NotEqual(a + b, value);
            Assert.Equal(FeatureAttribute.ReplacedReturn, value);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(() => (Task)instance.NonRetry_Exception());
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRetry_Failed, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.RetryException.GetType(), async () => await (ValueTask<string>)instance.NonRetry_ExceptionRetry());
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRetry_Failed, actual);

            actual.Clear();
            var v2 = await (Task<string>)instance.NonRetry_ExceptionHandled();
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRetry_Failed, actual);
            Assert.Equal(FeatureAttribute.ReplacedReturn, v2);

            actual.Clear();
            var v5 = await (ValueTask<string>)instance.NonRetry_SuccessRetry();
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRetry, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = await (Task<string>)instance.NonRetry_SuccessReplaced();
            Assert.Equal(FeatureAsyncUseCase.Expect_NonRetry, actual);
            Assert.Equal(FeatureAttribute.ReplacedReturn, v4);
        }

        [Fact]
        public async Task AsyncFeatureOnEntryExitTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await (ValueTask<string>)instance.OnEntryExit(string.Empty);
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntryExit, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = await (Task<Guid>)instance.OnEntryExit_RewriteArgs(guid1);
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntryExit, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = await (ValueTask<string>)instance.OnEntryExit_EntryReplace(a, b);
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntryExit, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(() => (Task)instance.OnEntryExit_Exception());
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntryExit, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.RetryException.GetType(), async () => await (ValueTask<string>)instance.OnEntryExit_ExceptionRetry());
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntryExit, actual);

            actual.Clear();
            await Assert.ThrowsAsync(FeatureAttribute.HandleableException.GetType(), () => (Task<string>)instance.OnEntryExit_ExceptionHandled());
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntryExit, actual);

            actual.Clear();
            var v5 = await (ValueTask<string>)instance.OnEntryExit_SuccessRetry();
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntryExit, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = await (Task<string>)instance.OnEntryExit_SuccessReplaced();
            Assert.Equal(FeatureAsyncUseCase.Expect_OnEntryExit, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public async Task DifferentFeatureApplyTest()
        {
            var instance = GetInstance("FeatureAsyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(() => (Task<string>)instance.DiffApplyVariables());
            Assert.Equal(FeatureAsyncUseCase.Expect_DiffApplyVariables, actual);

            actual.Clear();
            await Assert.ThrowsAsync<NotImplementedException>(async () => await (ValueTask<string>)instance.DiffApplyArray());
            Assert.Equal(FeatureAsyncUseCase.Expect_DiffApplyArray, actual);
        }
    }
}
