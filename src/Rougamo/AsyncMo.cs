using Rougamo.Context;
using System.Threading.Tasks;

namespace Rougamo
{
    /// <inheritdoc/>
    public abstract class AsyncMo : RawMo
    {
        /// <inheritdoc/>
        public override ValueTask OnEntryAsync(MethodContext context) => default;

        /// <inheritdoc/>
        public override ValueTask OnExceptionAsync(MethodContext context) => default;

        /// <inheritdoc/>
        public override ValueTask OnSuccessAsync(MethodContext context) => default;

        /// <inheritdoc/>
        public override ValueTask OnExitAsync(MethodContext context) => default;

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
