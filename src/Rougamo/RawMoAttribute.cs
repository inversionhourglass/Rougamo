using Rougamo.Context;
using System;
using System.Threading.Tasks;

namespace Rougamo
{
    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
    public abstract class RawMoAttribute : Attribute, IMo
    {
        /// <inheritdoc/>
        public abstract void OnEntry(MethodContext context);

        /// <inheritdoc/>
        public abstract ValueTask OnEntryAsync(MethodContext context);

        /// <inheritdoc/>
        public abstract void OnException(MethodContext context);

        /// <inheritdoc/>
        public abstract ValueTask OnExceptionAsync(MethodContext context);

        /// <inheritdoc/>
        public abstract void OnExit(MethodContext context);

        /// <inheritdoc/>
        public abstract ValueTask OnExitAsync(MethodContext context);

        /// <inheritdoc/>
        public abstract void OnSuccess(MethodContext context);

        /// <inheritdoc/>
        public abstract ValueTask OnSuccessAsync(MethodContext context);
    }
}
