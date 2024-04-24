using BasicUsage.Attributes;
using Rougamo;
using System;
using System.Collections.Generic;

namespace BasicUsage
{
    public class FeatureIteratorUseCase : IRecording
    {
        public List<string> Recording { get; } = new List<string>();

        #region OnEntry
        public static string[] Expect_OnEntry = new[] { "OnEntry-1" };

        [Feature(1, Features = Feature.OnEntry)]
        public IEnumerable<string> OnEntry(string value)
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            yield return value;
        }

        [Feature(1, Features = Feature.OnEntry)]
        public IEnumerable<string> OnEntry_RewriteArgs(Guid guid)
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            yield return guid.ToString();
        }

        [Feature(1, Features = Feature.OnEntry)]
        public IEnumerable<string> OnEntry_Exception()
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            throw new NotImplementedException();
        }
        #endregion OnEntry

        #region OnException
        public static string[] Expect_OnException = new[] { "OnException-1" };

        [Feature(1, Features = Feature.OnException)]
        public IEnumerable<string> OnException(string value)
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            //yield return value;
        }

        [Feature(1, Features = Feature.OnException)]
        public IEnumerable<string> OnException_RewriteArgs(Guid guid)
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            yield return guid.ToString();
        }

        [Feature(1, Features = Feature.OnException)]
        public IEnumerable<string> OnException_Exception()
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            throw new NotImplementedException();
        }
        #endregion OnException

        #region OnSuccess
        public static string[] Expect_OnSuccess = new[] { "OnSuccess-1" };

        [Feature(1, Features = Feature.OnSuccess)]
        public IEnumerable<string> OnSuccess(string value)
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            yield return value;
        }

        [Feature(1, Features = Feature.OnSuccess)]
        public IEnumerable<string> OnSuccess_RewriteArgs(Guid guid)
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            yield return guid.ToString();
        }

        [Feature(1, Features = Feature.OnSuccess)]
        public IEnumerable<string> OnSuccess_Exception()
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            throw new NotImplementedException();
        }
        #endregion OnSuccess

        #region OnExit
        public static string[] Expect_OnExit = new[] { "OnExit-1" };

        [Feature(1, Features = Feature.OnExit)]
        public IEnumerable<string> OnExit(string value)
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            yield return value;
        }

        [Feature(1, Features = Feature.OnExit)]
        public IEnumerable<string> OnExit_RewriteArgs(Guid guid)
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            yield return guid.ToString();
        }

        [Feature(1, Features = Feature.OnExit)]
        public IEnumerable<string> OnExit_Exception()
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            throw new NotImplementedException();
        }
        #endregion OnExit

        #region RewriteArgs
        public static string[] Expect_RewriteArgs = new[] { "OnEntry-1" };

        [Feature(1, Features = Feature.RewriteArgs)]
        public IEnumerable<string> RewriteArgs(string value)
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            yield return value;
        }

        [Feature(1, Features = Feature.RewriteArgs)]
        public IEnumerable<string> RewriteArgs_RewriteArgs(Guid guid)
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            yield return guid.ToString();
        }

        [Feature(1, Features = Feature.RewriteArgs)]
        public IEnumerable<string> RewriteArgs_Exception()
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            throw new NotImplementedException();
        }
        #endregion RewriteArgs

        #region OnEntry and OnExit
        public static string[] Expect_OnEntryExit = new[] { "OnEntry-1", "OnExit-1" };

        [Feature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public IEnumerable<string> OnEntryExit(string value)
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            yield return value;
        }

        [Feature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public IEnumerable<string> OnEntryExit_RewriteArgs(Guid guid)
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            yield return guid.ToString();
        }

        [Feature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public IEnumerable<string> OnEntryExit_Exception()
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            throw new NotImplementedException();
        }
        #endregion OnEntry and OnExit

        #region Different Feature Apply
        public static string[] Expect_DiffApplyVariables = new[] { "OnEntry-1", "OnEntry-3", "OnException-3", "OnException-2" };
        [Feature(1, Features = Feature.OnEntry)]
        [Feature(2, Features = Feature.OnException)]
        [Feature(3, Features = Feature.OnEntry | Feature.OnException)]
        public IEnumerable<string> DiffApplyVariables()
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            throw new NotImplementedException();
        }

        public static string[] Expect_DiffApplyArray = new[] { "OnEntry-1", "OnEntry-3", "OnEntry-4", "OnEntry-5", "OnException-4", "OnException-2" };
        [Feature(1, Features = Feature.OnEntry)]
        [Feature(2, Features = Feature.OnException)]
        [Feature(3, Features = Feature.OnEntry)]
        [Feature(4, Features = Feature.OnEntry | Feature.OnException)]
        [Feature(5, Features = Feature.OnEntry)]
        public IEnumerable<string> DiffApplyArray()
        {
            yield return nameof(FeatureIteratorUseCase);
            yield return nameof(DiffApplyArray);
            throw new NotImplementedException();
        }
        #endregion Different Feature Apply
    }
}
