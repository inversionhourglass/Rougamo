using System;

namespace Rougamo
{
    /// <summary>
    /// Ignore code weaving.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class IgnoreMoAttribute : Attribute
    {
        /// <summary>
        /// Ignore the specified weaving type that implements the <see cref="IMo"/> interface, ignore all if null.
        /// </summary>
        public Type[]? MoTypes { get; set; }
    }
}
