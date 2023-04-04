using BasicUsage;
using BasicUsage.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

namespace Rougamo.Fody.Tests
{
    [Collection(nameof(BasicUsage))]
    public class FeatureTests : TestBase
    {
        public FeatureTests() : base("BasicUsage.dll")
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
            Assert.Throws(SyncFeatureAttribute.RetryException.GetType(), () => { instance.OnEntry_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntry, actual);

            actual.Clear();
            Assert.Throws(SyncFeatureAttribute.HandleableException.GetType(), () => { instance.OnEntry_ExceptionHandled(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntry, actual);

            actual.Clear();
            instance.OnEntry_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntry, actual);

            actual.Clear();
            instance.OnEntry_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntry, actual);
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
            Assert.Throws(SyncFeatureAttribute.RetryException.GetType(), () => { instance.OnException_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnException, actual);

            actual.Clear();
            Assert.Throws(SyncFeatureAttribute.HandleableException.GetType(), () => { instance.OnException_ExceptionHandled(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnException, actual);

            actual.Clear();
            instance.OnException_SuccessRetry();
            Assert.Empty(actual);

            actual.Clear();
            instance.OnException_SuccessReplaced();
            Assert.Empty(actual);
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
            Assert.Throws(SyncFeatureAttribute.RetryException.GetType(), () => { instance.OnSuccess_ExceptionRetry(); });
            Assert.Empty(actual);

            actual.Clear();
            Assert.Throws(SyncFeatureAttribute.HandleableException.GetType(), () => { instance.OnSuccess_ExceptionHandled(); });
            Assert.Empty(actual);

            actual.Clear();
            instance.OnSuccess_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_OnSuccess, actual);

            actual.Clear();
            instance.OnSuccess_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_OnSuccess, actual);
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
            Assert.Throws(SyncFeatureAttribute.RetryException.GetType(), () => { instance.OnExit_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnExit, actual);

            actual.Clear();
            Assert.Throws(SyncFeatureAttribute.HandleableException.GetType(), () => { instance.OnExit_ExceptionHandled(); });
            Assert.Equal(FeatureSyncUseCase.Expect_OnExit, actual);

            actual.Clear();
            instance.OnExit_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_OnExit, actual);

            actual.Clear();
            instance.OnExit_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_OnExit, actual);
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
            Assert.Throws(SyncFeatureAttribute.RetryException.GetType(), () => { instance.RewriteArgs_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_RewriteArgs, actual);

            actual.Clear();
            Assert.Throws(SyncFeatureAttribute.HandleableException.GetType(), () => { instance.RewriteArgs_ExceptionHandled(); });
            Assert.Equal(FeatureSyncUseCase.Expect_RewriteArgs, actual);

            actual.Clear();
            instance.RewriteArgs_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_RewriteArgs, actual);

            actual.Clear();
            instance.RewriteArgs_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_RewriteArgs, actual);
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
            Assert.Equal(SyncFeatureAttribute.ReplacedReturn, value);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { instance.EntryReplace_Exception(); });
            Assert.Equal(FeatureSyncUseCase.Expect_EntryReplace, actual);

            actual.Clear();
            Assert.Throws(SyncFeatureAttribute.RetryException.GetType(), () => { instance.EntryReplace_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_EntryReplace, actual);

            actual.Clear();
            Assert.Throws(SyncFeatureAttribute.HandleableException.GetType(), () => { instance.EntryReplace_ExceptionHandled(); });
            Assert.Equal(FeatureSyncUseCase.Expect_EntryReplace, actual);

            actual.Clear();
            instance.EntryReplace_SuccessRetry();
            Assert.Equal(FeatureSyncUseCase.Expect_EntryReplace, actual);

            actual.Clear();
            instance.EntryReplace_SuccessReplaced();
            Assert.Equal(FeatureSyncUseCase.Expect_EntryReplace, actual);
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
            Assert.Throws(SyncFeatureAttribute.RetryException.GetType(), () => { instance.ExceptionHandle_ExceptionRetry(); });
            Assert.Equal(FeatureSyncUseCase.Expect_ExceptionHandle, actual);

            actual.Clear();
            var v2 = (string)instance.ExceptionHandle_ExceptionHandled();
            Assert.Equal(FeatureSyncUseCase.Expect_ExceptionHandle, actual);
            Assert.Equal(SyncFeatureAttribute.ReplacedReturn, v2);

            actual.Clear();
            instance.ExceptionHandle_SuccessRetry();
            Assert.Empty(actual);

            actual.Clear();
            instance.ExceptionHandle_SuccessReplaced();
            Assert.Empty(actual);
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
