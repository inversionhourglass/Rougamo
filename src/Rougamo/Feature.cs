using System;
using System.ComponentModel;

namespace Rougamo
{
#pragma warning disable CS0618 // Type or member is obsolete
    /// <summary>
    /// </summary>
    [Flags]
    public enum Feature
    {
        /// <summary>
        /// OnEntry
        /// </summary>
        OnEntry = 0x1,
        /// <summary>
        /// OnException
        /// </summary>
        OnException = 0x2,
        /// <summary>
        /// OnSuccess
        /// </summary>
        OnSuccess = 0x4,
        /// <summary>
        /// OnExit
        /// </summary>
        OnExit = 0x8,
        /// <summary>
        /// OnEntry and RewriteArgument
        /// </summary>
        RewriteArgs = 0x10 | OnEntry,
        /// <summary>
        /// OnEntry and return early with replaced return value
        /// </summary>
        EntryReplace = 0x20 | OnEntry,
        /// <summary>
        /// ExceptionRetry or SuccessRetry, you shouldn't use it in your code
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("you shouldn't use it in your code, it is used by rougamo")]
        RetryAny = 0x40,
        /// <summary>
        /// OnException and re-execute method if retry
        /// </summary>
        ExceptionRetry = RetryAny | OnException,
        /// <summary>
        /// OnSuccess and re-execute method if retry
        /// </summary>
        SuccessRetry = RetryAny | OnSuccess,
        /// <summary>
        /// ExceptionRetry or SuccessRetry
        /// </summary>
        Retry = ExceptionRetry | SuccessRetry,
        /// <summary>
        /// OnException and return with handled value
        /// </summary>
        ExceptionHandle = 0x80 | OnException,
        /// <summary>
        /// OnSuccess and return replaced value
        /// </summary>
        SuccessReplace = 0x100 | OnSuccess,
        /// <summary>
        /// OnEntry, OnException, OnSuccess and OnExit
        /// </summary>
        Observe = OnEntry | OnException | OnSuccess | OnExit,
        /// <summary>
        /// Except Rewrite Arguments
        /// </summary>
        NonRewriteArgs = (All ^ 0x10) & All,
        /// <summary>
        /// Except Retry
        /// </summary>
        NonRetry = (All ^ RetryAny) & All,
        /// <summary>
        /// all features(default)
        /// </summary>
        All = Observe | RewriteArgs | EntryReplace | RetryAny | ExceptionHandle | SuccessReplace
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
