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
        public string OnEntry_ExceptionRetry()
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
        public string OnException_ExceptionRetry()
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
        public string OnSuccess_ExceptionRetry()
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
        public string OnExit_ExceptionRetry()
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
        public string RewriteArgs_ExceptionRetry()
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
        public string EntryReplace_ExceptionRetry()
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
        public string ExceptionHandle_ExceptionRetry()
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

        #region SuccessReplace
        public static string[] Expect_SuccessReplace = new[] { "OnSuccess-1" };

        [SyncFeature(1, Features = Feature.SuccessReplace)]
        public string SuccessReplace(string value)
        {
            return value + Guid.NewGuid().ToString();
        }

        [SyncFeature(1, Features = Feature.SuccessReplace)]
        public Guid SuccessReplace_RewriteArgs(Guid guid)
        {
            return guid;
        }

        [SyncFeature(1, Features = Feature.SuccessReplace)]
        public string SuccessReplace_EntryReplace(string x, string y)
        {
            return x + y;
        }

        [SyncFeature(1, Features = Feature.SuccessReplace)]
        public void SuccessReplace_Exception()
        {
            throw new NotImplementedException();
        }

        [SyncFeature(1, Features = Feature.SuccessReplace)]
        public string SuccessReplace_ExceptionRetry()
        {
            throw SyncFeatureAttribute.RetryException;
        }

        [SyncFeature(1, Features = Feature.SuccessReplace)]
        public string SuccessReplace_ExceptionHandled()
        {
            throw SyncFeatureAttribute.HandleableException;
        }

        [SyncFeature(1, Features = Feature.SuccessReplace)]
        public string SuccessReplace_SuccessRetry()
        {
            return SyncFeatureAttribute.RetryReturn;
        }

        [SyncFeature(1, Features = Feature.SuccessReplace)]
        public string SuccessReplace_SuccessReplaced()
        {
            return SyncFeatureAttribute.ReplaceableReturn;
        }
        #endregion SuccessReplace

        #region SuccessRetry
        public static string[] Expect_SuccessRetry = new[] { "OnSuccess-1" };

        [SyncFeature(1, Features = Feature.SuccessRetry)]
        public string SuccessRetry(string value)
        {
            return value + Guid.NewGuid().ToString();
        }

        [SyncFeature(1, Features = Feature.SuccessRetry)]
        public Guid SuccessRetry_RewriteArgs(Guid guid)
        {
            return guid;
        }

        [SyncFeature(1, Features = Feature.SuccessRetry)]
        public string SuccessRetry_EntryReplace(string x, string y)
        {
            return x + y;
        }

        [SyncFeature(1, Features = Feature.SuccessRetry)]
        public void SuccessRetry_Exception()
        {
            throw new NotImplementedException();
        }

        [SyncFeature(1, Features = Feature.SuccessRetry)]
        public string SuccessRetry_ExceptionRetry()
        {
            throw SyncFeatureAttribute.RetryException;
        }

        [SyncFeature(1, Features = Feature.SuccessRetry)]
        public string SuccessRetry_ExceptionHandled()
        {
            throw SyncFeatureAttribute.HandleableException;
        }

        public static string[] Expect_SuccessRetried = new[] { "OnSuccess-1", "OnSuccess-1" };
        [SyncFeature(1, Features = Feature.SuccessRetry)]
        public string SuccessRetry_SuccessRetry()
        {
            return SyncFeatureAttribute.RetryReturn;
        }

        [SyncFeature(1, Features = Feature.SuccessRetry)]
        public string SuccessRetry_SuccessReplaced()
        {
            return SyncFeatureAttribute.ReplaceableReturn;
        }
        #endregion SuccessRetry

        #region Observe
        public static string[] Expect_Observe = new[] { "OnEntry-1", "OnSuccess-1", "OnExit-1" };
        public static string[] Expect_Observe_Failed = new[] { "OnEntry-1", "OnException-1", "OnExit-1" };

        [SyncFeature(1, Features = Feature.Observe)]
        public string Observe(string value)
        {
            return value + Guid.NewGuid().ToString();
        }

        [SyncFeature(1, Features = Feature.Observe)]
        public Guid Observe_RewriteArgs(Guid guid)
        {
            return guid;
        }

        [SyncFeature(1, Features = Feature.Observe)]
        public string Observe_EntryReplace(string x, string y)
        {
            return x + y;
        }

        [SyncFeature(1, Features = Feature.Observe)]
        public void Observe_Exception()
        {
            throw new NotImplementedException();
        }

        [SyncFeature(1, Features = Feature.Observe)]
        public string Observe_ExceptionRetry()
        {
            throw SyncFeatureAttribute.RetryException;
        }

        [SyncFeature(1, Features = Feature.Observe)]
        public string Observe_ExceptionHandled()
        {
            throw SyncFeatureAttribute.HandleableException;
        }

        [SyncFeature(1, Features = Feature.Observe)]
        public string Observe_SuccessRetry()
        {
            return SyncFeatureAttribute.RetryReturn;
        }

        [SyncFeature(1, Features = Feature.Observe)]
        public string Observe_SuccessReplaced()
        {
            return SyncFeatureAttribute.ReplaceableReturn;
        }
        #endregion Observe

        #region NonRewriteArgs
        public static string[] Expect_NonRewriteArgs = new[] { "OnEntry-1", "OnSuccess-1", "OnExit-1" };
        public static string[] Expect_NonRewriteArgs_EntryReplaced = new[] { "OnEntry-1", "OnExit-1" };
        public static string[] Expect_NonRewriteArgs_Failed = new[] { "OnEntry-1", "OnException-1", "OnExit-1" };
        public static string[] Expect_NonRewriteArgs_ExceptionRetried = new[] { "OnEntry-1", "OnException-1", "OnException-1", "OnExit-1" };
        public static string[] Expect_NonRewriteArgs_SuccessRetried = new[] { "OnEntry-1", "OnSuccess-1", "OnSuccess-1", "OnExit-1" };

        [SyncFeature(1, Features = Feature.NonRewriteArgs)]
        public string NonRewriteArgs(string value)
        {
            return value + Guid.NewGuid().ToString();
        }

        [SyncFeature(1, Features = Feature.NonRewriteArgs)]
        public Guid NonRewriteArgs_RewriteArgs(Guid guid)
        {
            return guid;
        }

        [SyncFeature(1, Features = Feature.NonRewriteArgs)]
        public string NonRewriteArgs_EntryReplace(string x, string y)
        {
            return x + y;
        }

        [SyncFeature(1, Features = Feature.NonRewriteArgs)]
        public void NonRewriteArgs_Exception()
        {
            throw new NotImplementedException();
        }

        [SyncFeature(1, Features = Feature.NonRewriteArgs)]
        public string NonRewriteArgs_ExceptionRetry()
        {
            throw SyncFeatureAttribute.RetryException;
        }

        [SyncFeature(1, Features = Feature.NonRewriteArgs)]
        public string NonRewriteArgs_ExceptionHandled()
        {
            throw SyncFeatureAttribute.HandleableException;
        }

        [SyncFeature(1, Features = Feature.NonRewriteArgs)]
        public string NonRewriteArgs_SuccessRetry()
        {
            return SyncFeatureAttribute.RetryReturn;
        }

        [SyncFeature(1, Features = Feature.NonRewriteArgs)]
        public string NonRewriteArgs_SuccessReplaced()
        {
            return SyncFeatureAttribute.ReplaceableReturn;
        }
        #endregion NonRewriteArgs

        #region NonRetry
        public static string[] Expect_NonRetry = new[] { "OnEntry-1", "OnSuccess-1", "OnExit-1" };
        public static string[] Expect_NonRetry_EntryReplaced = new[] { "OnEntry-1", "OnExit-1" };
        public static string[] Expect_NonRetry_Failed = new[] { "OnEntry-1", "OnException-1", "OnExit-1" };

        [SyncFeature(1, Features = Feature.NonRetry)]
        public string NonRetry(string value)
        {
            return value + Guid.NewGuid().ToString();
        }

        [SyncFeature(1, Features = Feature.NonRetry)]
        public Guid NonRetry_RewriteArgs(Guid guid)
        {
            return guid;
        }

        [SyncFeature(1, Features = Feature.NonRetry)]
        public string NonRetry_EntryReplace(string x, string y)
        {
            return x + y;
        }

        [SyncFeature(1, Features = Feature.NonRetry)]
        public void NonRetry_Exception()
        {
            throw new NotImplementedException();
        }

        [SyncFeature(1, Features = Feature.NonRetry)]
        public string NonRetry_ExceptionRetry()
        {
            throw SyncFeatureAttribute.RetryException;
        }

        [SyncFeature(1, Features = Feature.NonRetry)]
        public string NonRetry_ExceptionHandled()
        {
            throw SyncFeatureAttribute.HandleableException;
        }

        [SyncFeature(1, Features = Feature.NonRetry)]
        public string NonRetry_SuccessRetry()
        {
            return SyncFeatureAttribute.RetryReturn;
        }

        [SyncFeature(1, Features = Feature.NonRetry)]
        public string NonRetry_SuccessReplaced()
        {
            return SyncFeatureAttribute.ReplaceableReturn;
        }
        #endregion NonRetry

        #region OnEntry and OnExit
        public static string[] Expect_OnEntryExit = new[] { "OnEntry-1", "OnExit-1" };

        [SyncFeature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public string OnEntryExit(string value)
        {
            return value + Guid.NewGuid().ToString();
        }

        [SyncFeature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public Guid OnEntryExit_RewriteArgs(Guid guid)
        {
            return guid;
        }

        [SyncFeature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public string OnEntryExit_EntryReplace(string x, string y)
        {
            return x + y;
        }

        [SyncFeature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public void OnEntryExit_Exception()
        {
            throw new NotImplementedException();
        }

        [SyncFeature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public string OnEntryExit_ExceptionRetry()
        {
            throw SyncFeatureAttribute.RetryException;
        }

        [SyncFeature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public string OnEntryExit_ExceptionHandled()
        {
            throw SyncFeatureAttribute.HandleableException;
        }

        [SyncFeature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public string OnEntryExit_SuccessRetry()
        {
            return SyncFeatureAttribute.RetryReturn;
        }

        [SyncFeature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public string OnEntryExit_SuccessReplaced()
        {
            return SyncFeatureAttribute.ReplaceableReturn;
        }
        #endregion OnEntry and OnExit

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
