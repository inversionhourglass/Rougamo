using BasicUsage.Attributes;
using Rougamo;
using System;
using System.Collections.Generic;

namespace BasicUsage
{
    public class FeatureSyncUseCase : IRecording
    {
        public List<string> Recording { get; } = new List<string>();

        #region OnEntry
        public static string[] Expect_OnEntry = new[] { "OnEntry-1" };

        [SyncFeature(1, Features = Feature.OnEntry)]
        public string OnEntry(string value)
        {
            return value + Guid.NewGuid().ToString();
        }

        [SyncFeature(1, Features = Feature.OnEntry)]
        public Guid OnEntry_RewriteArgs(Guid guid)
        {
            return guid;
        }

        [SyncFeature(1, Features = Feature.OnEntry)]
        public string OnEntry_EntryReplace(string x, string y)
        {
            return x + y;
        }

        [SyncFeature(1, Features = Feature.OnEntry)]
        public void OnEntry_Exception()
        {
            throw new NotImplementedException();
        }

        [SyncFeature(1, Features = Feature.OnEntry)]
        public void OnEntry_ExceptionRetry()
        {
            throw SyncFeatureAttribute.RetryException;
        }

        [SyncFeature(1, Features = Feature.OnEntry)]
        public void OnEntry_ExceptionHandled()
        {
            throw SyncFeatureAttribute.HandleableException;
        }

        [SyncFeature(1, Features = Feature.OnEntry)]
        public string OnEntry_SuccessRetry()
        {
            return SyncFeatureAttribute.RetryReturn;
        }

        [SyncFeature(1, Features = Feature.OnEntry)]
        public string OnEntry_SuccessReplaced()
        {
            return SyncFeatureAttribute.ReplaceableReturn;
        }
        #endregion OnEntry

        #region OnException
        public static string[] Expect_OnException = new[] { "OnException-1" };

        [SyncFeature(1, Features = Feature.OnException)]
        public string OnException(string value)
        {
            return value + Guid.NewGuid().ToString();
        }

        [SyncFeature(1, Features = Feature.OnException)]
        public Guid OnException_RewriteArgs(Guid guid)
        {
            return guid;
        }

        [SyncFeature(1, Features = Feature.OnException)]
        public string OnException_EntryReplace(string x, string y)
        {
            return x + y;
        }

        [SyncFeature(1, Features = Feature.OnException)]
        public void OnException_Exception()
        {
            throw new NotImplementedException();
        }

        [SyncFeature(1, Features = Feature.OnException)]
        public void OnException_ExceptionRetry()
        {
            throw SyncFeatureAttribute.RetryException;
        }

        [SyncFeature(1, Features = Feature.OnException)]
        public void OnException_ExceptionHandled()
        {
            throw SyncFeatureAttribute.HandleableException;
        }

        [SyncFeature(1, Features = Feature.OnException)]
        public string OnException_SuccessRetry()
        {
            return SyncFeatureAttribute.RetryReturn;
        }

        [SyncFeature(1, Features = Feature.OnException)]
        public string OnException_SuccessReplaced()
        {
            return SyncFeatureAttribute.ReplaceableReturn;
        }
        #endregion OnException

        #region OnSuccess
        public static string[] Expect_OnSuccess = new[] { "OnSuccess-1" };

        [SyncFeature(1, Features = Feature.OnSuccess)]
        public string OnSuccess(string value)
        {
            return value + Guid.NewGuid().ToString();
        }

        [SyncFeature(1, Features = Feature.OnSuccess)]
        public Guid OnSuccess_RewriteArgs(Guid guid)
        {
            return guid;
        }

        [SyncFeature(1, Features = Feature.OnSuccess)]
        public string OnSuccess_EntryReplace(string x, string y)
        {
            return x + y;
        }

        [SyncFeature(1, Features = Feature.OnSuccess)]
        public void OnSuccess_Exception()
        {
            throw new NotImplementedException();
        }

        [SyncFeature(1, Features = Feature.OnSuccess)]
        public void OnSuccess_ExceptionRetry()
        {
            throw SyncFeatureAttribute.RetryException;
        }

        [SyncFeature(1, Features = Feature.OnSuccess)]
        public void OnSuccess_ExceptionHandled()
        {
            throw SyncFeatureAttribute.HandleableException;
        }

        [SyncFeature(1, Features = Feature.OnSuccess)]
        public string OnSuccess_SuccessRetry()
        {
            return SyncFeatureAttribute.RetryReturn;
        }

        [SyncFeature(1, Features = Feature.OnSuccess)]
        public string OnSuccess_SuccessReplaced()
        {
            return SyncFeatureAttribute.ReplaceableReturn;
        }
        #endregion OnSuccess

        #region Different Feature Apply
        public static string[] Expect_DiffApplyVariables = new[] { "OnEntry-1", "OnEntry-3", "OnException-3", "OnException-2" };
        [SyncFeature(1, Features = Feature.OnEntry)]
        [SyncFeature(2, Features = Feature.OnException)]
        [SyncFeature(3, Features = Feature.OnEntry | Feature.OnException)]
        public string DiffApplyVariables()
        {
            throw new NotImplementedException();
        }

        public static string[] Expect_DiffApplyArray = new[] { "OnEntry-1", "OnEntry-3", "OnEntry-4", "OnEntry-5", "OnException-4", "OnException-2" };
        [SyncFeature(1, Features = Feature.OnEntry)]
        [SyncFeature(2, Features = Feature.OnException)]
        [SyncFeature(3, Features = Feature.OnEntry)]
        [SyncFeature(4, Features = Feature.OnEntry | Feature.OnException)]
        [SyncFeature(5, Features = Feature.OnEntry)]
        public string DiffApplyArray()
        {
            throw new NotImplementedException();
        }
        #endregion Different Feature Apply
    }
}
