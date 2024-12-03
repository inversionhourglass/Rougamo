#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using Rougamo.Context;

namespace Rougamo
{
    /// <summary>
    /// For the struct Mo, provide a default implementation of the sync aspect method.
    /// </summary>
    public interface IAsyncMo : IMo
    {
        /// <inheritdoc/>
        void IMo.OnEntry(MethodContext context)
        {
            OnEntryAsync(context).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        void IMo.OnSuccess(MethodContext context)
        {
            OnSuccessAsync(context).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        void IMo.OnException(MethodContext context)
        {
            OnExceptionAsync(context).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        void IMo.OnExit(MethodContext context)
        {
            OnExitAsync(context).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
#endif
