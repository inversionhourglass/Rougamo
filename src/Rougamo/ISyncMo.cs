#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using Rougamo.Context;
using System.Threading.Tasks;

namespace Rougamo
{
    /// <summary>
    /// For the struct Mo, provide a default implementation of the async aspect method.
    /// </summary>
    public interface ISyncMo : IMo
    {
        /// <inheritdoc />
        ValueTask IMo.OnEntryAsync(MethodContext context)
        {
            OnEntry(context);

            return default;
        }

        /// <inheritdoc />
        ValueTask IMo.OnSuccessAsync(MethodContext context)
        {
            OnSuccess(context);

            return default;
        }

        /// <inheritdoc />
        ValueTask IMo.OnExceptionAsync(MethodContext context)
        {
            OnException(context);

            return default;
        }

        /// <inheritdoc />
        ValueTask IMo.OnExitAsync(MethodContext context)
        {
            OnExit(context);

            return default;
        }
    }
}
#endif
