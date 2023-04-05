using BasicUsage;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rougamo.Fody.Tests
{
    [Collection(nameof(BasicUsage))]
    public class FeatureIteratorTests : TestBase
    {
        public FeatureIteratorTests() : base("BasicUsage.dll")
        {
        }

        protected override string RootNamespace => "BasicUsage";

        [Fact]
        public void IteratorFeatureOnEntryTest()
        {
            var instance = GetInstance("FeatureIteratorUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            var p1 = nameof(IteratorFeatureOnEntryTest);
            var v1 = ((IEnumerable<string>)instance.OnEntry(p1)).ToArray();
            Assert.Equal(FeatureIteratorUseCase.Expect_OnEntry, actual);
            Assert.Equal(p1, v1.Last());

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var v2 = ((IEnumerable<string>)instance.OnEntry_RewriteArgs(guid1)).ToArray();
            Assert.Equal(FeatureIteratorUseCase.Expect_OnEntry, actual);
            Assert.Equal(guid1.ToString(), v2.Last());

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { ((IEnumerable<string>)instance.OnEntry_Exception()).ToArray(); });
            Assert.Equal(FeatureIteratorUseCase.Expect_OnEntry, actual);
        }

        [Fact]
        public void IteratorFeatureOnExceptionTest()
        {
            var instance = GetInstance("FeatureIteratorUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            var p1 = nameof(IteratorFeatureOnEntryTest);
            var v1 = ((IEnumerable<string>)instance.OnException(p1)).ToArray();
            Assert.Empty(actual);
            Assert.Equal(p1, v1.Last());

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var v2 = ((IEnumerable<string>)instance.OnException_RewriteArgs(guid1)).ToArray();
            Assert.Empty(actual);
            Assert.Equal(guid1.ToString(), v2.Last());

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { ((IEnumerable<string>)instance.OnException_Exception()).ToArray(); });
            Assert.Equal(FeatureIteratorUseCase.Expect_OnException, actual);
        }

        [Fact]
        public void IteratorFeatureOnSuccessTest()
        {
            var instance = GetInstance("FeatureIteratorUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            var p1 = nameof(IteratorFeatureOnEntryTest);
            var v1 = ((IEnumerable<string>)instance.OnSuccess(p1)).ToArray();
            Assert.Equal(FeatureIteratorUseCase.Expect_OnSuccess, actual);
            Assert.Equal(p1, v1.Last());

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var v2 = ((IEnumerable<string>)instance.OnSuccess_RewriteArgs(guid1)).ToArray();
            Assert.Equal(FeatureIteratorUseCase.Expect_OnSuccess, actual);
            Assert.Equal(guid1.ToString(), v2.Last());

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { ((IEnumerable<string>)instance.OnSuccess_Exception()).ToArray(); });
            Assert.Empty(actual);
        }

        [Fact]
        public void IteratorFeatureOnExitTest()
        {
            var instance = GetInstance("FeatureIteratorUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            var p1 = nameof(IteratorFeatureOnEntryTest);
            var v1 = ((IEnumerable<string>)instance.OnExit(p1)).ToArray();
            Assert.Equal(FeatureIteratorUseCase.Expect_OnExit, actual);
            Assert.Equal(p1, v1.Last());

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var v2 = ((IEnumerable<string>)instance.OnExit_RewriteArgs(guid1)).ToArray();
            Assert.Equal(FeatureIteratorUseCase.Expect_OnExit, actual);
            Assert.Equal(guid1.ToString(), v2.Last());

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { ((IEnumerable<string>)instance.OnExit_Exception()).ToArray(); });
            Assert.Equal(FeatureIteratorUseCase.Expect_OnExit, actual);
        }

        [Fact]
        public void IteratorFeatureRewriteArgsTest()
        {
            var instance = GetInstance("FeatureIteratorUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            var p1 = nameof(IteratorFeatureOnEntryTest);
            var v1 = ((IEnumerable<string>)instance.RewriteArgs(p1)).ToArray();
            Assert.Equal(FeatureIteratorUseCase.Expect_RewriteArgs, actual);
            Assert.Equal(p1, v1.Last());

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var v2 = ((IEnumerable<string>)instance.RewriteArgs_RewriteArgs(guid1)).ToArray();
            Assert.Equal(FeatureIteratorUseCase.Expect_RewriteArgs, actual);
            Assert.NotEqual(guid1.ToString(), v2.Last());

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { ((IEnumerable<string>)instance.RewriteArgs_Exception()).ToArray(); });
            Assert.Equal(FeatureIteratorUseCase.Expect_RewriteArgs, actual);
        }

        [Fact]
        public void IteratorFeatureOnEntryExitTest()
        {
            var instance = GetInstance("FeatureIteratorUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            var p1 = nameof(IteratorFeatureOnEntryTest);
            var v1 = ((IEnumerable<string>)instance.OnEntryExit(p1)).ToArray();
            Assert.Equal(FeatureIteratorUseCase.Expect_OnEntryExit, actual);
            Assert.Equal(p1, v1.Last());

            actual.Clear();
            var guid1 = Guid.NewGuid();
            var v2 = ((IEnumerable<string>)instance.OnEntryExit_RewriteArgs(guid1)).ToArray();
            Assert.Equal(FeatureIteratorUseCase.Expect_OnEntryExit, actual);
            Assert.Equal(guid1.ToString(), v2.Last());
            actual.Clear();

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => { ((IEnumerable<string>)instance.OnEntryExit_Exception()).ToArray(); });
            Assert.Equal(FeatureIteratorUseCase.Expect_OnEntryExit, actual);
        }

        [Fact]
        public void DifferentFeatureApplyTest()
        {
            var instance = GetInstance("FeatureIteratorUseCase");
            var actual = (List<string>)instance.Recording;

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => ((IEnumerable<string>)instance.DiffApplyVariables()).ToArray());
            Assert.Equal(FeatureIteratorUseCase.Expect_DiffApplyVariables, actual);

            actual.Clear();
            Assert.Throws<NotImplementedException>(() => ((IEnumerable<string>)instance.DiffApplyArray()).ToArray());
            Assert.Equal(FeatureIteratorUseCase.Expect_DiffApplyArray, actual);
        }
    }
}
