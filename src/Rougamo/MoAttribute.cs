using Rougamo.Context;
using System;

namespace Rougamo
{
    /// <inheritdoc/>
    [AttributeUsage(AttributeTargets.Assembly|AttributeTargets.Module|AttributeTargets.Class|AttributeTargets.Method)]
    public abstract class MoAttribute : Attribute, IMo
    {
        /// <inheritdoc/>
        public virtual AccessFlags Flags { get; }

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
