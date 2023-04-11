using Rougamo.Context;
using System;

namespace Rougamo
{
    /// <inheritdoc/>
    [AttributeUsage(AttributeTargets.Assembly|AttributeTargets.Module|AttributeTargets.Class|AttributeTargets.Method|AttributeTargets.Property)]
    public abstract class MoAttribute : Attribute, IMo
    {
        /// <inheritdoc/>
        public virtual AccessFlags Flags { get; set; }

        /// <inheritdoc/>
        public virtual Feature Features { get; set; }

        /// <inheritdoc/>
        public virtual double Order { get; set; }

        /// <inheritdoc/>
        public virtual void OnEntry(MethodContext context) { }

        /// <inheritdoc/>
        public virtual void OnException(MethodContext context) { }

        /// <inheritdoc/>
        public virtual void OnExit(MethodContext context) { }

        /// <inheritdoc/>
        public virtual void OnSuccess(MethodContext context) { }
    }
}
