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
            instance.OnEntry_RewriteArgs(Guid.NewGuid());
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntry, actual);

            actual.Clear();
            instance.OnEntry_EntryReplace(string.Empty, null);
            Assert.Equal(FeatureSyncUseCase.Expect_OnEntry, actual);

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
            instance.OnException_RewriteArgs(Guid.NewGuid());
            Assert.Empty(actual);

            actual.Clear();
            instance.OnException_EntryReplace(string.Empty, null);
            Assert.Empty(actual);

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
            instance.OnSuccess_RewriteArgs(Guid.NewGuid());
            Assert.Equal(FeatureSyncUseCase.Expect_OnSuccess, actual);

            actual.Clear();
            instance.OnSuccess_EntryReplace(string.Empty, null);
            Assert.Equal(FeatureSyncUseCase.Expect_OnSuccess, actual);

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
