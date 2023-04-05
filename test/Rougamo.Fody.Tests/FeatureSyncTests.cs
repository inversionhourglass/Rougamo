using BasicUsage;
using BasicUsage.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

namespace Rougamo.Fody.Tests
{
    [Collection(nameof(BasicUsage))]
    public class FeatureSyncTests : TestBase
    {
        public FeatureSyncTests() : base("BasicUsage.dll")
        {
        }

        protected override string RootNamespace => "BasicUsage";

        [Fact]
        public void SyncFeatureOnEntryTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            instance.OnEntry(string.Empty);
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntry, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = (Guid)instance.OnEntry_RewriteArgs(guid1);
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntry, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = instance.OnEntry_EntryReplace(a, b);
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntry, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.OnEntry_Exception(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntry, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.RetryException.GetType(), () => { instance.OnEntry_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntry, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.HandleableException.GetType(), () => { instance.OnEntry_ExceptionHandled(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntry, actual);

            actual.Clear();
            var v5 = (string)instance.OnEntry_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntry, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = (string)instance.OnEntry_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntry, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public void SyncFeatureOnExceptionTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            instance.OnException(string.Empty);
            Assert.Empty(actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = (Guid)instance.OnException_RewriteArgs(guid1);
            Assert.Empty(actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = instance.OnException_EntryReplace(a, b);
            Assert.Empty(actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.OnException_Exception(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnException, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.RetryException.GetType(), () => { instance.OnException_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnException, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.HandleableException.GetType(), () => { instance.OnException_ExceptionHandled(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnException, actual);

            actual.Clear();
            var v5 = (string)instance.OnException_SuccessRetry();
            Assert.Empty(actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = (string)instance.OnException_SuccessReplaced();
            Assert.Empty(actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public void SyncFeatureOnSuccessTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            instance.OnSuccess(string.Empty);
            Assert.Equal(FeatureSyncUseCase.Expect_OnSuccess, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = (Guid)instance.OnSuccess_RewriteArgs(guid1);
            Assert.Equal(FeatureSyncUseCase.Expect_OnSuccess, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = instance.OnSuccess_EntryReplace(a, b);
            Assert.Equal(FeatureSyncUseCase.Expect_OnSuccess, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.OnSuccess_Exception(); });
            Assert.Empty(actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.RetryException.GetType(), () => { instance.OnSuccess_ExceptionRetry(); });
            Assert.Empty(actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.HandleableException.GetType(), () => { instance.OnSuccess_ExceptionHandled(); });
            Assert.Empty(actual);

            actual.Clear();
            var v5 = (string)instance.OnSuccess_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_OnSuccess, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = (string)instance.OnSuccess_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_OnSuccess, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public void SyncFeatureOnExitTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            instance.OnExit(string.Empty);
            Assert.Equal(FeatureSyncUseCase.Expect_OnExit, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = (Guid)instance.OnExit_RewriteArgs(guid1);
            Assert.Equal(FeatureSyncUseCase.Expect_OnExit, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = instance.OnExit_EntryReplace(a, b);
            Assert.Equal(FeatureSyncUseCase.Expect_OnExit, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.OnExit_Exception(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnExit, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.RetryException.GetType(), () => { instance.OnExit_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnExit, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.HandleableException.GetType(), () => { instance.OnExit_ExceptionHandled(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnExit, actual);

            actual.Clear();
            var v5 = (string)instance.OnExit_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_OnExit, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = (string)instance.OnExit_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_OnExit, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public void SyncFeatureRewriteArgsTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            instance.RewriteArgs(string.Empty);
            Assert.Equal(FeatureSyncUseCase.Expect_RewriteArgs, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = (Guid)instance.RewriteArgs_RewriteArgs(guid1);
            Assert.Equal(FeatureSyncUseCase.Expect_RewriteArgs, actual);
            Assert.NotEqual(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = instance.RewriteArgs_EntryReplace(a, b);
            Assert.Equal(FeatureSyncUseCase.Expect_RewriteArgs, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.RewriteArgs_Exception(); });
            Assert.Equal(FeatureSyncUseCase.Expect_RewriteArgs, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.RetryException.GetType(), () => { instance.RewriteArgs_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_RewriteArgs, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.HandleableException.GetType(), () => { instance.RewriteArgs_ExceptionHandled(); });
            Assert.Equal(FeatureSyncUseCase.Expect_RewriteArgs, actual);

            actual.Clear();
            var v5 = (string)instance.RewriteArgs_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_RewriteArgs, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = (string)instance.RewriteArgs_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_RewriteArgs, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public void SyncFeatureEntryReplaceTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            instance.EntryReplace(string.Empty);
            Assert.Equal(FeatureSyncUseCase.Expect_EntryReplace, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = (Guid)instance.EntryReplace_RewriteArgs(guid1);
            Assert.Equal(FeatureSyncUseCase.Expect_EntryReplace, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = instance.EntryReplace_EntryReplace(a, b);
            Assert.Equal(FeatureSyncUseCase.Expect_EntryReplace, actual);
            Assert.NotEqual(a + b, value);
            Assert.Equal(FeatureAttribute.ReplacedReturn, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.EntryReplace_Exception(); });
            Assert.Equal(FeatureSyncUseCase.Expect_EntryReplace, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.RetryException.GetType(), () => { instance.EntryReplace_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_EntryReplace, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.HandleableException.GetType(), () => { instance.EntryReplace_ExceptionHandled(); });
            Assert.Equal(FeatureSyncUseCase.Expect_EntryReplace, actual);

            actual.Clear();
            var v5 = (string)instance.EntryReplace_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_EntryReplace, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = (string)instance.EntryReplace_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_EntryReplace, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public void SyncFeatureExceptionHandleTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            instance.ExceptionHandle(string.Empty);
            Assert.Empty(actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = (Guid)instance.ExceptionHandle_RewriteArgs(guid1);
            Assert.Empty(actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = instance.ExceptionHandle_EntryReplace(a, b);
            Assert.Empty(actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.ExceptionHandle_Exception(); });
            Assert.Equal(FeatureSyncUseCase.Expect_ExceptionHandle, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.RetryException.GetType(), () => { instance.ExceptionHandle_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_ExceptionHandle, actual);

            actual.Clear();
            var v2 = (string)instance.ExceptionHandle_ExceptionHandled();
            Assert.Equal(FeatureSyncUseCase.Expect_ExceptionHandle, actual);
            Assert.Equal(FeatureAttribute.ReplacedReturn, v2);

            actual.Clear();
            var v5 = (string)instance.ExceptionHandle_SuccessRetry();
            Assert.Empty(actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = (string)instance.ExceptionHandle_SuccessReplaced();
            Assert.Empty(actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public void SyncFeatureExceptionRetryTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            instance.ExceptionRetry(string.Empty);
            Assert.Empty(actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = (Guid)instance.ExceptionRetry_RewriteArgs(guid1);
            Assert.Empty(actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = instance.ExceptionRetry_EntryReplace(a, b);
            Assert.Empty(actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.ExceptionRetry_Exception(); });
            Assert.Equal(FeatureSyncUseCase.Expect_ExceptionRetry, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.RetryException.GetType(), () => { instance.ExceptionRetry_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_ExceptionRetried, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.HandleableException.GetType(), () => { instance.ExceptionRetry_ExceptionHandled(); });
            Assert.Equal(FeatureSyncUseCase.Expect_ExceptionRetry, actual);

            actual.Clear();
            var v5 = (string)instance.ExceptionRetry_SuccessRetry();
            Assert.Empty(actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = (string)instance.ExceptionRetry_SuccessReplaced();
            Assert.Empty(actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public void SyncFeatureSuccessReplaceTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            instance.SuccessReplace(string.Empty);
            Assert.Equal(FeatureSyncUseCase.Expect_SuccessReplace, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = (Guid)instance.SuccessReplace_RewriteArgs(guid1);
            Assert.Equal(FeatureSyncUseCase.Expect_SuccessReplace, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = instance.SuccessReplace_EntryReplace(a, b);
            Assert.Equal(FeatureSyncUseCase.Expect_SuccessReplace, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.SuccessReplace_Exception(); });
            Assert.Empty(actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.RetryException.GetType(), () => { instance.SuccessReplace_ExceptionRetry(); });
            Assert.Empty(actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.HandleableException.GetType(), () => { instance.SuccessReplace_ExceptionHandled(); });
            Assert.Empty(actual);

            actual.Clear();
            var v5 = (string)instance.SuccessReplace_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_SuccessReplace, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = (string)instance.SuccessReplace_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_SuccessReplace, actual);
            Assert.Equal(FeatureAttribute.ReplacedReturn, v4);
        }

        [Fact]
        public void SyncFeatureSuccessRetryTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            instance.SuccessRetry(string.Empty);
            Assert.Equal(FeatureSyncUseCase.Expect_SuccessRetry, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = (Guid)instance.SuccessRetry_RewriteArgs(guid1);
            Assert.Equal(FeatureSyncUseCase.Expect_SuccessRetry, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = instance.SuccessRetry_EntryReplace(a, b);
            Assert.Equal(FeatureSyncUseCase.Expect_SuccessRetry, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.SuccessRetry_Exception(); });
            Assert.Empty(actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.RetryException.GetType(), () => { instance.SuccessRetry_ExceptionRetry(); });
            Assert.Empty(actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.HandleableException.GetType(), () => { instance.SuccessRetry_ExceptionHandled(); });
            Assert.Empty(actual);

            actual.Clear();
            var v5 = (string)instance.SuccessRetry_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_SuccessRetried, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = (string)instance.SuccessRetry_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_SuccessRetry, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public void SyncFeatureObserveTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            instance.Observe(string.Empty);
            Assert.Equal(FeatureSyncUseCase.Expect_Observe, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = (Guid)instance.Observe_RewriteArgs(guid1);
            Assert.Equal(FeatureSyncUseCase.Expect_Observe, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = instance.Observe_EntryReplace(a, b);
            Assert.Equal(FeatureSyncUseCase.Expect_Observe, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.Observe_Exception(); });
            Assert.Equal(FeatureSyncUseCase.Expect_Observe_Failed, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.RetryException.GetType(), () => { instance.Observe_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_Observe_Failed, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.HandleableException.GetType(), () => { instance.Observe_ExceptionHandled(); });
            Assert.Equal(FeatureSyncUseCase.Expect_Observe_Failed, actual);

            actual.Clear();
            var v5 = (string)instance.Observe_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_Observe, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = (string)instance.Observe_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_Observe, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public void SyncFeatureNonRewriteArgsTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            instance.NonRewriteArgs(string.Empty);
            Assert.Equal(FeatureSyncUseCase.Expect_NonRewriteArgs, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = (Guid)instance.NonRewriteArgs_RewriteArgs(guid1);
            Assert.Equal(FeatureSyncUseCase.Expect_NonRewriteArgs, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = instance.NonRewriteArgs_EntryReplace(a, b);
            Assert.Equal(FeatureSyncUseCase.Expect_NonRewriteArgs_EntryReplaced, actual);
            Assert.NotEqual(a + b, value);
            Assert.Equal(FeatureAttribute.ReplacedReturn, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.NonRewriteArgs_Exception(); });
            Assert.Equal(FeatureSyncUseCase.Expect_NonRewriteArgs_Failed, actual);

            actual.Clear();
            var v6 = (string)instance.NonRewriteArgs_ExceptionRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_NonRewriteArgs_ExceptionRetried, actual);
            Assert.Equal(FeatureAttribute.RetriedReturn, v6);

            actual.Clear();
            var v2 = instance.NonRewriteArgs_ExceptionHandled();
            Assert.Equal(FeatureSyncUseCase.Expect_NonRewriteArgs_Failed, actual);
            Assert.Equal(FeatureAttribute.ReplacedReturn, v2);

            actual.Clear();
            var v5 = (string)instance.NonRewriteArgs_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_NonRewriteArgs_SuccessRetried, actual);
            Assert.Equal(FeatureAttribute.RetriedReturn, v5);

            actual.Clear();
            var v4 = (string)instance.NonRewriteArgs_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_NonRewriteArgs, actual);
            Assert.Equal(FeatureAttribute.ReplacedReturn, v4);
        }

        [Fact]
        public void SyncFeatureNonRetryTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            instance.NonRetry(string.Empty);
            Assert.Equal(FeatureSyncUseCase.Expect_NonRetry, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = (Guid)instance.NonRetry_RewriteArgs(guid1);
            Assert.Equal(FeatureSyncUseCase.Expect_NonRetry, actual);
            Assert.NotEqual(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = instance.NonRetry_EntryReplace(a, b);
            Assert.Equal(FeatureSyncUseCase.Expect_NonRetry_EntryReplaced, actual);
            Assert.NotEqual(a + b, value);
            Assert.Equal(FeatureAttribute.ReplacedReturn, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.NonRetry_Exception(); });
            Assert.Equal(FeatureSyncUseCase.Expect_NonRetry_Failed, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.RetryException.GetType(), () => { instance.NonRetry_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_NonRetry_Failed, actual);

            actual.Clear();
            var v2 = instance.NonRetry_ExceptionHandled();
            Assert.Equal(FeatureSyncUseCase.Expect_NonRetry_Failed, actual);
            Assert.Equal(FeatureAttribute.ReplacedReturn, v2);

            actual.Clear();
            var v5 = (string)instance.NonRetry_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_NonRetry, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = (string)instance.NonRetry_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_NonRetry, actual);
            Assert.Equal(FeatureAttribute.ReplacedReturn, v4);
        }

        [Fact]
        public void SyncFeatureOnEntryExitTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            instance.OnEntryExit(string.Empty);
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntryExit, actual);

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var guid2 = (Guid)instance.OnEntryExit_RewriteArgs(guid1);
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntryExit, actual);
            Assert.Equal(guid1, guid2);

            actual.Clear();
            var a = "a";
            var b = "b";
            var value = instance.OnEntryExit_EntryReplace(a, b);
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntryExit, actual);
            Assert.Equal(a + b, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.OnEntryExit_Exception(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntryExit, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.RetryException.GetType(), () => { instance.OnEntryExit_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntryExit, actual);

            actual.Clear();
            Assert.Throws(FeatureAttribute.HandleableException.GetType(), () => { instance.OnEntryExit_ExceptionHandled(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntryExit, actual);

            actual.Clear();
            var v5 = (string)instance.OnEntryExit_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntryExit, actual);
            Assert.Equal(FeatureAttribute.RetryReturn, v5);

            actual.Clear();
            var v4 = (string)instance.OnEntryExit_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntryExit, actual);
            Assert.Equal(FeatureAttribute.ReplaceableReturn, v4);
        }

        [Fact]
        public void DifferentFeatureApplyTest()
        {
            var instance = GetInstance("FeatureSyncUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => instance.DiffApplyVariables());
            Assert.Equal(FeatureSyncUseCase.Expect_DiffApplyVariables, actual);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => instance.DiffApplyArray());
            Assert.Equal(FeatureSyncUseCase.Expect_DiffApplyArray, actual);
        }
    }
}
