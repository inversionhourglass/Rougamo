using Rougamo.Context;
using System;

namespace Rougamo
{
    /// <inheritdoc/>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
    public abstract class AsyncMoAttribute : RawMoAttribute
    {
        /// <inheritdoc/>
        public sealed override void OnEntry(MethodContext context)
        {
            OnEntryAsync(context).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public sealed override void OnException(MethodContext context)
        {
            OnExceptionAsync(context).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public sealed override void OnSuccess(MethodContext context)
        {
            OnSuccessAsync(context).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public sealed override void OnExit(MethodContext context)
        {
            OnExitAsync(context).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
