using Rougamo.Context;
using System.Threading.Tasks;

namespace Rougamo
{
    /// <inheritdoc/>
    public abstract class Mo : RawMo
    {
        /// <inheritdoc/>
        public override void OnEntry(MethodContext context) { }

        /// <inheritdoc/>
        public override void OnException(MethodContext context) { }

        /// <inheritdoc/>
        public override void OnSuccess(MethodContext context) { }

        /// <inheritdoc/>
        public override void OnExit(MethodContext context) { }

        /// <inheritdoc/>
        public sealed override ValueTask OnEntryAsync(MethodContext context)
        {
            OnEntry(context);
            return default;
        }

        /// <inheritdoc/>
        public sealed override ValueTask OnExceptionAsync(MethodContext context)
        {
            OnException(context);
            return default;
        }

        /// <inheritdoc/>
        public sealed override ValueTask OnSuccessAsync(MethodContext context)
        {
            OnSuccess(context);
            return default;
        }

        /// <inheritdoc/>
        public sealed override ValueTask OnExitAsync(MethodContext context)
        {
            OnExit(context);
            return default;
        }
    }
}
