using BasicUsage.Attributes;
using Rougamo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class FeatureAsyncUseCase : IRecording
    {
        public List<string> Recording { get; } = new List<string>();

        #region OnEntry
        public static string[] Expect_OnEntry = new[] { "OnEntry-1" };

        [Feature(1, Features = Feature.OnEntry)]
        public async Task<string> OnEntry(string value)
        {
            await Task.Yield();
            return value + Guid.NewGuid().ToString();
        }

        [Feature(1, Features = Feature.OnEntry)]
        public async ValueTask<Guid> OnEntry_RewriteArgs(Guid guid)
        {
            await Task.Yield();
            return guid;
        }

        [Feature(1, Features = Feature.OnEntry)]
        public async Task<string> OnEntry_EntryReplace(string x, string y)
        {
            await Task.Yield();
            return x + y;
        }

        [Feature(1, Features = Feature.OnEntry)]
        public async ValueTask OnEntry_Exception()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        [Feature(1, Features = Feature.OnEntry)]
        public async Task<string> OnEntry_ExceptionRetry()
        {
            await Task.Yield();
            throw FeatureAttribute.RetryException;
        }

        [Feature(1, Features = Feature.OnEntry)]
        public async ValueTask<string> OnEntry_ExceptionHandled()
        {
            await Task.Yield();
            throw FeatureAttribute.HandleableException;
        }

        [Feature(1, Features = Feature.OnEntry)]
        public async Task<string> OnEntry_SuccessRetry()
        {
            await Task.Yield();
            return FeatureAttribute.RetryReturn;
        }

        [Feature(1, Features = Feature.OnEntry)]
        public async ValueTask<string> OnEntry_SuccessReplaced()
        {
            await Task.Yield();
            return FeatureAttribute.ReplaceableReturn;
        }
        #endregion OnEntry

        #region OnException
        public static string[] Expect_OnException = new[] { "OnException-1" };

        [Feature(1, Features = Feature.OnException)]
        public async Task<string> OnException(string value)
        {
            await Task.Yield();
            return value + Guid.NewGuid().ToString();
        }

        [Feature(1, Features = Feature.OnException)]
        public async ValueTask<Guid> OnException_RewriteArgs(Guid guid)
        {
            await Task.Yield();
            return guid;
        }

        [Feature(1, Features = Feature.OnException)]
        public async Task<string> OnException_EntryReplace(string x, string y)
        {
            await Task.Yield();
            return x + y;
        }

        [Feature(1, Features = Feature.OnException)]
        public async ValueTask OnException_Exception()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        [Feature(1, Features = Feature.OnException)]
        public async Task<string> OnException_ExceptionRetry()
        {
            await Task.Yield();
            throw FeatureAttribute.RetryException;
        }

        [Feature(1, Features = Feature.OnException)]
        public async ValueTask<string> OnException_ExceptionHandled()
        {
            await Task.Yield();
            throw FeatureAttribute.HandleableException;
        }

        [Feature(1, Features = Feature.OnException)]
        public async Task<string> OnException_SuccessRetry()
        {
            await Task.Yield();
            return FeatureAttribute.RetryReturn;
        }

        [Feature(1, Features = Feature.OnException)]
        public async ValueTask<string> OnException_SuccessReplaced()
        {
            await Task.Yield();
            return FeatureAttribute.ReplaceableReturn;
        }
        #endregion OnException

        #region OnSuccess
        public static string[] Expect_OnSuccess = new[] { "OnSuccess-1" };

        [Feature(1, Features = Feature.OnSuccess)]
        public async Task<string> OnSuccess(string value)
        {
            await Task.Yield();
            return value + Guid.NewGuid().ToString();
        }

        [Feature(1, Features = Feature.OnSuccess)]
        public async ValueTask<Guid> OnSuccess_RewriteArgs(Guid guid)
        {
            await Task.Yield();
            return guid;
        }

        [Feature(1, Features = Feature.OnSuccess)]
        public async Task<string> OnSuccess_EntryReplace(string x, string y)
        {
            await Task.Yield();
            return x + y;
        }

        [Feature(1, Features = Feature.OnSuccess)]
        public async ValueTask OnSuccess_Exception()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        [Feature(1, Features = Feature.OnSuccess)]
        public async Task<string> OnSuccess_ExceptionRetry()
        {
            await Task.Yield();
            throw FeatureAttribute.RetryException;
        }

        [Feature(1, Features = Feature.OnSuccess)]
        public async ValueTask<string> OnSuccess_ExceptionHandled()
        {
            await Task.Yield();
            throw FeatureAttribute.HandleableException;
        }

        [Feature(1, Features = Feature.OnSuccess)]
        public async Task<string> OnSuccess_SuccessRetry()
        {
            await Task.Yield();
            return FeatureAttribute.RetryReturn;
        }

        [Feature(1, Features = Feature.OnSuccess)]
        public async ValueTask<string> OnSuccess_SuccessReplaced()
        {
            await Task.Yield();
            return FeatureAttribute.ReplaceableReturn;
        }
        #endregion OnSuccess

        #region OnExit
        public static string[] Expect_OnExit = new[] { "OnExit-1" };

        [Feature(1, Features = Feature.OnExit)]
        public async Task<string> OnExit(string value)
        {
            await Task.Yield();
            return value + Guid.NewGuid().ToString();
        }

        [Feature(1, Features = Feature.OnExit)]
        public async ValueTask<Guid> OnExit_RewriteArgs(Guid guid)
        {
            await Task.Yield();
            return guid;
        }

        [Feature(1, Features = Feature.OnExit)]
        public async Task<string> OnExit_EntryReplace(string x, string y)
        {
            await Task.Yield();
            return x + y;
        }

        [Feature(1, Features = Feature.OnExit)]
        public async ValueTask OnExit_Exception()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        [Feature(1, Features = Feature.OnExit)]
        public async Task<string> OnExit_ExceptionRetry()
        {
            await Task.Yield();
            throw FeatureAttribute.RetryException;
        }

        [Feature(1, Features = Feature.OnExit)]
        public async ValueTask<string> OnExit_ExceptionHandled()
        {
            await Task.Yield();
            throw FeatureAttribute.HandleableException;
        }

        [Feature(1, Features = Feature.OnExit)]
        public async Task<string> OnExit_SuccessRetry()
        {
            await Task.Yield();
            return FeatureAttribute.RetryReturn;
        }

        [Feature(1, Features = Feature.OnExit)]
        public async ValueTask<string> OnExit_SuccessReplaced()
        {
            await Task.Yield();
            return FeatureAttribute.ReplaceableReturn;
        }
        #endregion OnExit

        #region RewriteArgs
        public static string[] Expect_RewriteArgs = new[] { "OnEntry-1" };

        [Feature(1, Features = Feature.RewriteArgs)]
        public async Task<string> RewriteArgs(string value)
        {
            await Task.Yield();
            return value + Guid.NewGuid().ToString();
        }

        [Feature(1, Features = Feature.RewriteArgs)]
        public async ValueTask<Guid> RewriteArgs_RewriteArgs(Guid guid)
        {
            await Task.Yield();
            return guid;
        }

        [Feature(1, Features = Feature.RewriteArgs)]
        public async Task<string> RewriteArgs_EntryReplace(string x, string y)
        {
            await Task.Yield();
            return x + y;
        }

        [Feature(1, Features = Feature.RewriteArgs)]
        public async ValueTask RewriteArgs_Exception()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        [Feature(1, Features = Feature.RewriteArgs)]
        public async Task<string> RewriteArgs_ExceptionRetry()
        {
            await Task.Yield();
            throw FeatureAttribute.RetryException;
        }

        [Feature(1, Features = Feature.RewriteArgs)]
        public async ValueTask<string> RewriteArgs_ExceptionHandled()
        {
            await Task.Yield();
            throw FeatureAttribute.HandleableException;
        }

        [Feature(1, Features = Feature.RewriteArgs)]
        public async Task<string> RewriteArgs_SuccessRetry()
        {
            await Task.Yield();
            return FeatureAttribute.RetryReturn;
        }

        [Feature(1, Features = Feature.RewriteArgs)]
        public async ValueTask<string> RewriteArgs_SuccessReplaced()
        {
            await Task.Yield();
            return FeatureAttribute.ReplaceableReturn;
        }
        #endregion RewriteArgs

        #region EntryReplace
        public static string[] Expect_EntryReplace = new[] { "OnEntry-1" };

        [Feature(1, Features = Feature.EntryReplace)]
        public async Task<string> EntryReplace(string value)
        {
            await Task.Yield();
            return value + Guid.NewGuid().ToString();
        }

        [Feature(1, Features = Feature.EntryReplace)]
        public async ValueTask<Guid> EntryReplace_RewriteArgs(Guid guid)
        {
            await Task.Yield();
            return guid;
        }

        [Feature(1, Features = Feature.EntryReplace)]
        public async Task<string> EntryReplace_EntryReplace(string x, string y)
        {
            await Task.Yield();
            return x + y;
        }

        [Feature(1, Features = Feature.EntryReplace)]
        public async ValueTask EntryReplace_Exception()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        [Feature(1, Features = Feature.EntryReplace)]
        public async Task<string> EntryReplace_ExceptionRetry()
        {
            await Task.Yield();
            throw FeatureAttribute.RetryException;
        }

        [Feature(1, Features = Feature.EntryReplace)]
        public async ValueTask<string> EntryReplace_ExceptionHandled()
        {
            await Task.Yield();
            throw FeatureAttribute.HandleableException;
        }

        [Feature(1, Features = Feature.EntryReplace)]
        public async Task<string> EntryReplace_SuccessRetry()
        {
            await Task.Yield();
            return FeatureAttribute.RetryReturn;
        }

        [Feature(1, Features = Feature.EntryReplace)]
        public async ValueTask<string> EntryReplace_SuccessReplaced()
        {
            await Task.Yield();
            return FeatureAttribute.ReplaceableReturn;
        }
        #endregion EntryReplace

        #region ExceptionHandle
        public static string[] Expect_ExceptionHandle = new[] { "OnException-1" };

        [Feature(1, Features = Feature.ExceptionHandle)]
        public async ValueTask<string> ExceptionHandle(string value)
        {
            await Task.Yield();
            return value + Guid.NewGuid().ToString();
        }

        [Feature(1, Features = Feature.ExceptionHandle)]
        public async Task<Guid> ExceptionHandle_RewriteArgs(Guid guid)
        {
            await Task.Yield();
            return guid;
        }

        [Feature(1, Features = Feature.ExceptionHandle)]
        public async ValueTask<string> ExceptionHandle_EntryReplace(string x, string y)
        {
            await Task.Yield();
            return x + y;
        }

        [Feature(1, Features = Feature.ExceptionHandle)]
        public async Task ExceptionHandle_Exception()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        [Feature(1, Features = Feature.ExceptionHandle)]
        public async ValueTask<string> ExceptionHandle_ExceptionRetry()
        {
            await Task.Yield();
            throw FeatureAttribute.RetryException;
        }

        [Feature(1, Features = Feature.ExceptionHandle)]
        public async Task<string> ExceptionHandle_ExceptionHandled()
        {
            await Task.Yield();
            throw FeatureAttribute.HandleableException;
        }

        [Feature(1, Features = Feature.ExceptionHandle)]
        public async ValueTask<string> ExceptionHandle_SuccessRetry()
        {
            await Task.Yield();
            return FeatureAttribute.RetryReturn;
        }

        [Feature(1, Features = Feature.ExceptionHandle)]
        public async Task<string> ExceptionHandle_SuccessReplaced()
        {
            await Task.Yield();
            return FeatureAttribute.ReplaceableReturn;
        }
        #endregion ExceptionHandle

        #region ExceptionRetry
        public static string[] Expect_ExceptionRetry = new[] { "OnException-1" };

        [Feature(1, Features = Feature.ExceptionRetry)]
        public async ValueTask<string> ExceptionRetry(string value)
        {
            await Task.Yield();
            return value + Guid.NewGuid().ToString();
        }

        [Feature(1, Features = Feature.ExceptionRetry)]
        public async Task<Guid> ExceptionRetry_RewriteArgs(Guid guid)
        {
            await Task.Yield();
            return guid;
        }

        [Feature(1, Features = Feature.ExceptionRetry)]
        public async ValueTask<string> ExceptionRetry_EntryReplace(string x, string y)
        {
            await Task.Yield();
            return x + y;
        }

        [Feature(1, Features = Feature.ExceptionRetry)]
        public async Task ExceptionRetry_Exception()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        public static string[] Expect_ExceptionRetried = new[] { "OnException-1", "OnException-1" };
        [Feature(1, Features = Feature.ExceptionRetry)]
        public async ValueTask<string> ExceptionRetry_ExceptionRetry()
        {
            await Task.Yield();
            throw FeatureAttribute.RetryException;
        }

        [Feature(1, Features = Feature.ExceptionRetry)]
        public async Task<string> ExceptionRetry_ExceptionHandled()
        {
            await Task.Yield();
            throw FeatureAttribute.HandleableException;
        }

        [Feature(1, Features = Feature.ExceptionRetry)]
        public async ValueTask<string> ExceptionRetry_SuccessRetry()
        {
            await Task.Yield();
            return FeatureAttribute.RetryReturn;
        }

        [Feature(1, Features = Feature.ExceptionRetry)]
        public async Task<string> ExceptionRetry_SuccessReplaced()
        {
            await Task.Yield();
            return FeatureAttribute.ReplaceableReturn;
        }
        #endregion ExceptionRetry

        #region SuccessReplace
        public static string[] Expect_SuccessReplace = new[] { "OnSuccess-1" };

        [Feature(1, Features = Feature.SuccessReplace)]
        public async ValueTask<string> SuccessReplace(string value)
        {
            await Task.Yield();
            return value + Guid.NewGuid().ToString();
        }

        [Feature(1, Features = Feature.SuccessReplace)]
        public async Task<Guid> SuccessReplace_RewriteArgs(Guid guid)
        {
            await Task.Yield();
            return guid;
        }

        [Feature(1, Features = Feature.SuccessReplace)]
        public async ValueTask<string> SuccessReplace_EntryReplace(string x, string y)
        {
            await Task.Yield();
            return x + y;
        }

        [Feature(1, Features = Feature.SuccessReplace)]
        public async Task SuccessReplace_Exception()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        [Feature(1, Features = Feature.SuccessReplace)]
        public async ValueTask<string> SuccessReplace_ExceptionRetry()
        {
            await Task.Yield();
            throw FeatureAttribute.RetryException;
        }

        [Feature(1, Features = Feature.SuccessReplace)]
        public async Task<string> SuccessReplace_ExceptionHandled()
        {
            await Task.Yield();
            throw FeatureAttribute.HandleableException;
        }

        [Feature(1, Features = Feature.SuccessReplace)]
        public async ValueTask<string> SuccessReplace_SuccessRetry()
        {
            await Task.Yield();
            return FeatureAttribute.RetryReturn;
        }

        [Feature(1, Features = Feature.SuccessReplace)]
        public async Task<string> SuccessReplace_SuccessReplaced()
        {
            await Task.Yield();
            return FeatureAttribute.ReplaceableReturn;
        }
        #endregion SuccessReplace

        #region SuccessRetry
        public static string[] Expect_SuccessRetry = new[] { "OnSuccess-1" };

        [Feature(1, Features = Feature.SuccessRetry)]
        public async ValueTask<string> SuccessRetry(string value)
        {
            await Task.Yield();
            return value + Guid.NewGuid().ToString();
        }

        [Feature(1, Features = Feature.SuccessRetry)]
        public async Task<Guid> SuccessRetry_RewriteArgs(Guid guid)
        {
            await Task.Yield();
            return guid;
        }

        [Feature(1, Features = Feature.SuccessRetry)]
        public async ValueTask<string> SuccessRetry_EntryReplace(string x, string y)
        {
            await Task.Yield();
            return x + y;
        }

        [Feature(1, Features = Feature.SuccessRetry)]
        public async Task SuccessRetry_Exception()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        [Feature(1, Features = Feature.SuccessRetry)]
        public async ValueTask<string> SuccessRetry_ExceptionRetry()
        {
            await Task.Yield();
            throw FeatureAttribute.RetryException;
        }

        [Feature(1, Features = Feature.SuccessRetry)]
        public async Task<string> SuccessRetry_ExceptionHandled()
        {
            await Task.Yield();
            throw FeatureAttribute.HandleableException;
        }

        public static string[] Expect_SuccessRetried = new[] { "OnSuccess-1", "OnSuccess-1" };
        [Feature(1, Features = Feature.SuccessRetry)]
        public async ValueTask<string> SuccessRetry_SuccessRetry()
        {
            await Task.Yield();
            return FeatureAttribute.RetryReturn;
        }

        [Feature(1, Features = Feature.SuccessRetry)]
        public async Task<string> SuccessRetry_SuccessReplaced()
        {
            await Task.Yield();
            return FeatureAttribute.ReplaceableReturn;
        }
        #endregion SuccessRetry

        #region Observe
        public static string[] Expect_Observe = new[] { "OnEntry-1", "OnSuccess-1", "OnExit-1" };
        public static string[] Expect_Observe_Failed = new[] { "OnEntry-1", "OnException-1", "OnExit-1" };

        [Feature(1, Features = Feature.Observe)]
        public async ValueTask<string> Observe(string value)
        {
            await Task.Yield();
            return value + Guid.NewGuid().ToString();
        }

        [Feature(1, Features = Feature.Observe)]
        public async Task<Guid> Observe_RewriteArgs(Guid guid)
        {
            await Task.Yield();
            return guid;
        }

        [Feature(1, Features = Feature.Observe)]
        public async ValueTask<string> Observe_EntryReplace(string x, string y)
        {
            await Task.Yield();
            return x + y;
        }

        [Feature(1, Features = Feature.Observe)]
        public async Task Observe_Exception()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        [Feature(1, Features = Feature.Observe)]
        public async ValueTask<string> Observe_ExceptionRetry()
        {
            await Task.Yield();
            throw FeatureAttribute.RetryException;
        }

        [Feature(1, Features = Feature.Observe)]
        public async Task<string> Observe_ExceptionHandled()
        {
            await Task.Yield();
            throw FeatureAttribute.HandleableException;
        }

        [Feature(1, Features = Feature.Observe)]
        public async ValueTask<string> Observe_SuccessRetry()
        {
            await Task.Yield();
            return FeatureAttribute.RetryReturn;
        }

        [Feature(1, Features = Feature.Observe)]
        public async Task<string> Observe_SuccessReplaced()
        {
            await Task.Yield();
            return FeatureAttribute.ReplaceableReturn;
        }
        #endregion Observe

        #region NonRewriteArgs
        public static string[] Expect_NonRewriteArgs = new[] { "OnEntry-1", "OnSuccess-1", "OnExit-1" };
        public static string[] Expect_NonRewriteArgs_EntryReplaced = new[] { "OnEntry-1", "OnExit-1" };
        public static string[] Expect_NonRewriteArgs_Failed = new[] { "OnEntry-1", "OnException-1", "OnExit-1" };
        public static string[] Expect_NonRewriteArgs_ExceptionRetried = new[] { "OnEntry-1", "OnException-1", "OnException-1", "OnExit-1" };
        public static string[] Expect_NonRewriteArgs_SuccessRetried = new[] { "OnEntry-1", "OnSuccess-1", "OnSuccess-1", "OnExit-1" };

        [Feature(1, Features = Feature.NonRewriteArgs)]
        public async ValueTask<string> NonRewriteArgs(string value)
        {
            await Task.Yield();
            return value + Guid.NewGuid().ToString();
        }

        [Feature(1, Features = Feature.NonRewriteArgs)]
        public async Task<Guid> NonRewriteArgs_RewriteArgs(Guid guid)
        {
            await Task.Yield();
            return guid;
        }

        [Feature(1, Features = Feature.NonRewriteArgs)]
        public async ValueTask<string> NonRewriteArgs_EntryReplace(string x, string y)
        {
            await Task.Yield();
            return x + y;
        }

        [Feature(1, Features = Feature.NonRewriteArgs)]
        public async Task NonRewriteArgs_Exception()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        [Feature(1, Features = Feature.NonRewriteArgs)]
        public async ValueTask<string> NonRewriteArgs_ExceptionRetry()
        {
            await Task.Yield();
            throw FeatureAttribute.RetryException;
        }

        [Feature(1, Features = Feature.NonRewriteArgs)]
        public async Task<string> NonRewriteArgs_ExceptionHandled()
        {
            await Task.Yield();
            throw FeatureAttribute.HandleableException;
        }

        [Feature(1, Features = Feature.NonRewriteArgs)]
        public async ValueTask<string> NonRewriteArgs_SuccessRetry()
        {
            await Task.Yield();
            return FeatureAttribute.RetryReturn;
        }

        [Feature(1, Features = Feature.NonRewriteArgs)]
        public async Task<string> NonRewriteArgs_SuccessReplaced()
        {
            await Task.Yield();
            return FeatureAttribute.ReplaceableReturn;
        }
        #endregion NonRewriteArgs

        #region NonRetry
        public static string[] Expect_NonRetry = new[] { "OnEntry-1", "OnSuccess-1", "OnExit-1" };
        public static string[] Expect_NonRetry_EntryReplaced = new[] { "OnEntry-1", "OnExit-1" };
        public static string[] Expect_NonRetry_Failed = new[] { "OnEntry-1", "OnException-1", "OnExit-1" };

        [Feature(1, Features = Feature.NonRetry)]
        public async ValueTask<string> NonRetry(string value)
        {
            await Task.Yield();
            return value + Guid.NewGuid().ToString();
        }

        [Feature(1, Features = Feature.NonRetry)]
        public async Task<Guid> NonRetry_RewriteArgs(Guid guid)
        {
            await Task.Yield();
            return guid;
        }

        [Feature(1, Features = Feature.NonRetry)]
        public async ValueTask<string> NonRetry_EntryReplace(string x, string y)
        {
            await Task.Yield();
            return x + y;
        }

        [Feature(1, Features = Feature.NonRetry)]
        public async Task NonRetry_Exception()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        [Feature(1, Features = Feature.NonRetry)]
        public async ValueTask<string> NonRetry_ExceptionRetry()
        {
            await Task.Yield();
            throw FeatureAttribute.RetryException;
        }

        [Feature(1, Features = Feature.NonRetry)]
        public async Task<string> NonRetry_ExceptionHandled()
        {
            await Task.Yield();
            throw FeatureAttribute.HandleableException;
        }

        [Feature(1, Features = Feature.NonRetry)]
        public async ValueTask<string> NonRetry_SuccessRetry()
        {
            await Task.Yield();
            return FeatureAttribute.RetryReturn;
        }

        [Feature(1, Features = Feature.NonRetry)]
        public async Task<string> NonRetry_SuccessReplaced()
        {
            await Task.Yield();
            return FeatureAttribute.ReplaceableReturn;
        }
        #endregion NonRetry

        #region OnEntry and OnExit
        public static string[] Expect_OnEntryExit = new[] { "OnEntry-1", "OnExit-1" };

        [Feature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public async ValueTask<string> OnEntryExit(string value)
        {
            await Task.Yield();
            return value + Guid.NewGuid().ToString();
        }

        [Feature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public async Task<Guid> OnEntryExit_RewriteArgs(Guid guid)
        {
            await Task.Yield();
            return guid;
        }

        [Feature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public async ValueTask<string> OnEntryExit_EntryReplace(string x, string y)
        {
            await Task.Yield();
            return x + y;
        }

        [Feature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public async Task OnEntryExit_Exception()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        [Feature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public async ValueTask<string> OnEntryExit_ExceptionRetry()
        {
            await Task.Yield();
            throw FeatureAttribute.RetryException;
        }

        [Feature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public async Task<string> OnEntryExit_ExceptionHandled()
        {
            await Task.Yield();
            throw FeatureAttribute.HandleableException;
        }

        [Feature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public async ValueTask<string> OnEntryExit_SuccessRetry()
        {
            await Task.Yield();
            return FeatureAttribute.RetryReturn;
        }

        [Feature(1, Features = Feature.OnEntry | Feature.OnExit)]
        public async Task<string> OnEntryExit_SuccessReplaced()
        {
            await Task.Yield();
            return FeatureAttribute.ReplaceableReturn;
        }
        #endregion OnEntry and OnExit

        #region Different Feature Apply
        public static string[] Expect_DiffApplyVariables = new[] { "OnEntry-1", "OnEntry-3", "OnException-3", "OnException-2" };
        [Feature(1, Features = Feature.OnEntry)]
        [Feature(2, Features = Feature.OnException)]
        [Feature(3, Features = Feature.OnEntry | Feature.OnException)]
        public async Task<string> DiffApplyVariables()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }

        public static string[] Expect_DiffApplyArray = new[] { "OnEntry-1", "OnEntry-3", "OnEntry-4", "OnEntry-5", "OnException-4", "OnException-2" };
        [Feature(1, Features = Feature.OnEntry)]
        [Feature(2, Features = Feature.OnException)]
        [Feature(3, Features = Feature.OnEntry)]
        [Feature(4, Features = Feature.OnEntry | Feature.OnException)]
        [Feature(5, Features = Feature.OnEntry)]
        public async ValueTask<string> DiffApplyArray()
        {
            await Task.Yield();
            throw new NotImplementedException();
        }
        #endregion Different Feature Apply
    }
}
