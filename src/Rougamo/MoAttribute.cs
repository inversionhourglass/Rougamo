using Rougamo.Context;

namespace Rougamo
{
    /// <inheritdoc/>
    public abstract class MoAttribute : IMo
    {
        /// <inheritdoc/>
        public abstract void OnEntry(EntryContext context);

        /// <inheritdoc/>
        public abstract void OnException(ExceptionContext context);

        /// <inheritdoc/>
        public abstract void OnExit(ExitContext context);

        /// <inheritdoc/>
        public abstract void OnSuccess(SuccessContext context);
    }
}
