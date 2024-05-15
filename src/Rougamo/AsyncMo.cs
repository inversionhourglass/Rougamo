using Rougamo.Context;

namespace Rougamo
{
    /// <inheritdoc/>
    public abstract class AsyncMo : RawMo
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
