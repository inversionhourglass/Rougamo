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
    }
}
