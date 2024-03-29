﻿using Rougamo.Context;
using System;

namespace Rougamo
{
    /// <inheritdoc/>
    [AttributeUsage(AttributeTargets.Assembly|AttributeTargets.Module|AttributeTargets.Class|AttributeTargets.Struct|AttributeTargets.Method|AttributeTargets.Property|AttributeTargets.Constructor)]
    public abstract class MoAttribute : Attribute, IMo
    {
        /// <inheritdoc/>
        public virtual AccessFlags Flags { get; }

        /// <inheritdoc/>
        public virtual string? Pattern { get; }

        /// <inheritdoc/>
        public virtual Feature Features { get; }

        /// <inheritdoc/>
        public virtual double Order { get; }

        /// <inheritdoc/>
        public virtual Omit MethodContextOmits { get; }

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
