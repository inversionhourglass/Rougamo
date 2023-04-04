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
        public string OnEntry_ExceptionHandled()
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
        public string OnException_ExceptionHandled()
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
        public string OnSuccess_ExceptionHandled()
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

        #region OnExit
        public static string[] Expect_OnExit = new[] { "OnExit-1" };

        [SyncFeature(1, Features = Feature.OnExit)]
        public string OnExit(string value)
        {
            return value + Guid.NewGuid().ToString();
        }

        [SyncFeature(1, Features = Feature.OnExit)]
        public Guid OnExit_RewriteArgs(Guid guid)
        {
            return guid;
        }

        [SyncFeature(1, Features = Feature.OnExit)]
        public string OnExit_EntryReplace(string x, string y)
        {
            return x + y;
        }

        [SyncFeature(1, Features = Feature.OnExit)]
        public void OnExit_Exception()
        {
            throw new NotImplementedException();
        }

        [SyncFeature(1, Features = Feature.OnExit)]
        public void OnExit_ExceptionRetry()
        {
            throw SyncFeatureAttribute.RetryException;
        }

        [SyncFeature(1, Features = Feature.OnExit)]
        public string OnExit_ExceptionHandled()
        {
            throw SyncFeatureAttribute.HandleableException;
        }

        [SyncFeature(1, Features = Feature.OnExit)]
        public string OnExit_SuccessRetry()
        {
            return SyncFeatureAttribute.RetryReturn;
        }

        [SyncFeature(1, Features = Feature.OnExit)]
        public string OnExit_SuccessReplaced()
        {
            return SyncFeatureAttribute.ReplaceableReturn;
        }
        #endregion OnExit

        #region RewriteArgs
        public static string[] Expect_RewriteArgs = new[] { "OnEntry-1" };

        [SyncFeature(1, Features = Feature.RewriteArgs)]
        public string RewriteArgs(string value)
        {
            return value + Guid.NewGuid().ToString();
        }

        [SyncFeature(1, Features = Feature.RewriteArgs)]
        public Guid RewriteArgs_RewriteArgs(Guid guid)
        {
            return guid;
        }

        [SyncFeature(1, Features = Feature.RewriteArgs)]
        public string RewriteArgs_EntryReplace(string x, string y)
        {
            return x + y;
        }

        [SyncFeature(1, Features = Feature.RewriteArgs)]
        public void RewriteArgs_Exception()
        {
            throw new NotImplementedException();
        }

        [SyncFeature(1, Features = Feature.RewriteArgs)]
        public void RewriteArgs_ExceptionRetry()
        {
            throw SyncFeatureAttribute.RetryException;
        }

        [SyncFeature(1, Features = Feature.RewriteArgs)]
        public string RewriteArgs_ExceptionHandled()
        {
            throw SyncFeatureAttribute.HandleableException;
        }

        [SyncFeature(1, Features = Feature.RewriteArgs)]
        public string RewriteArgs_SuccessRetry()
        {
            return SyncFeatureAttribute.RetryReturn;
        }

        [SyncFeature(1, Features = Feature.RewriteArgs)]
        public string RewriteArgs_SuccessReplaced()
        {
            return SyncFeatureAttribute.ReplaceableReturn;
        }
        #endregion RewriteArgs

        #region EntryReplace
        public static string[] Expect_EntryReplace = new[] { "OnEntry-1" };

        [SyncFeature(1, Features = Feature.EntryReplace)]
        public string EntryReplace(string value)
        {
            return value + Guid.NewGuid().ToString();
        }

        [SyncFeature(1, Features = Feature.EntryReplace)]
        public Guid EntryReplace_RewriteArgs(Guid guid)
        {
            return guid;
        }

        [SyncFeature(1, Features = Feature.EntryReplace)]
        public string EntryReplace_EntryReplace(string x, string y)
        {
            return x + y;
        }

        [SyncFeature(1, Features = Feature.EntryReplace)]
        public void EntryReplace_Exception()
        {
            throw new NotImplementedException();
        }

        [SyncFeature(1, Features = Feature.EntryReplace)]
        public void EntryReplace_ExceptionRetry()
        {
            throw SyncFeatureAttribute.RetryException;
        }

        [SyncFeature(1, Features = Feature.EntryReplace)]
        public string EntryReplace_ExceptionHandled()
        {
            throw SyncFeatureAttribute.HandleableException;
        }

        [SyncFeature(1, Features = Feature.EntryReplace)]
        public string EntryReplace_SuccessRetry()
        {
            return SyncFeatureAttribute.RetryReturn;
        }

        [SyncFeature(1, Features = Feature.EntryReplace)]
        public string EntryReplace_SuccessReplaced()
        {
            return SyncFeatureAttribute.ReplaceableReturn;
        }
        #endregion EntryReplace

        #region ExceptionHandle
        public static string[] Expect_ExceptionHandle = new[] { "OnException-1" };

        [SyncFeature(1, Features = Feature.ExceptionHandle)]
        public string ExceptionHandle(string value)
        {
            return value + Guid.NewGuid().ToString();
        }

        [SyncFeature(1, Features = Feature.ExceptionHandle)]
        public Guid ExceptionHandle_RewriteArgs(Guid guid)
        {
            return guid;
        }

        [SyncFeature(1, Features = Feature.ExceptionHandle)]
        public string ExceptionHandle_EntryReplace(string x, string y)
        {
            return x + y;
        }

        [SyncFeature(1, Features = Feature.ExceptionHandle)]
        public void ExceptionHandle_Exception()
        {
            throw new NotImplementedException();
        }

        [SyncFeature(1, Features = Feature.ExceptionHandle)]
        public void ExceptionHandle_ExceptionRetry()
        {
            throw SyncFeatureAttribute.RetryException;
        }

        [SyncFeature(1, Features = Feature.ExceptionHandle)]
        public string ExceptionHandle_ExceptionHandled()
        {
            throw SyncFeatureAttribute.HandleableException;
        }

        [SyncFeature(1, Features = Feature.ExceptionHandle)]
        public string ExceptionHandle_SuccessRetry()
        {
            return SyncFeatureAttribute.RetryReturn;
        }

        [SyncFeature(1, Features = Feature.ExceptionHandle)]
        public string ExceptionHandle_SuccessReplaced()
        {
            return SyncFeatureAttribute.ReplaceableReturn;
        }
        #endregion ExceptionHandle

        #region ExceptionRetry
        public static string[] Expect_ExceptionRetry = new[] { "OnException-1" };

        [SyncFeature(1, Features = Feature.ExceptionRetry)]
        public string ExceptionRetry(string value)
        {
            return value + Guid.NewGuid().ToString();
        }

        [SyncFeature(1, Features = Feature.ExceptionRetry)]
        public Guid ExceptionRetry_RewriteArgs(Guid guid)
        {
            return guid;
        }

        [SyncFeature(1, Features = Feature.ExceptionRetry)]
        public string ExceptionRetry_EntryReplace(string x, string y)
        {
            return x + y;
        }

        [SyncFeature(1, Features = Feature.ExceptionRetry)]
        public void ExceptionRetry_Exception()
        {
            throw new NotImplementedException();
        }

        public static string[] Expect_ExceptionRetried = new[] { "OnException-1", "OnException-1" };
        [SyncFeature(1, Features = Feature.ExceptionRetry)]
        public string ExceptionRetry_ExceptionRetry()
        {
            throw SyncFeatureAttribute.RetryException;
        }

        [SyncFeature(1, Features = Feature.ExceptionRetry)]
        public string ExceptionRetry_ExceptionHandled()
        {
            throw SyncFeatureAttribute.HandleableException;
        }

        [SyncFeature(1, Features = Feature.ExceptionRetry)]
        public string ExceptionRetry_SuccessRetry()
        {
            return SyncFeatureAttribute.RetryReturn;
        }

        [SyncFeature(1, Features = Feature.ExceptionRetry)]
        public string ExceptionRetry_SuccessReplaced()
        {
            return SyncFeatureAttribute.ReplaceableReturn;
        }
        #endregion ExceptionRetry

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
