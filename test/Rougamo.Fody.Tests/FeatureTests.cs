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
    }
}
