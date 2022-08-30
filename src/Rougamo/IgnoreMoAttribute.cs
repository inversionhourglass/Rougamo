using System;

namespace Rougamo
{
    /// <summary>
    /// A marked type or method will ignore code weaving.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly|AttributeTargets.Module|AttributeTargets.Class|AttributeTargets.Method, AllowMultiple = true)]
    public sealed class IgnoreMoAttribute : Attribute
    {
        /// <summary>
        /// Ignore the specified weaving type that implements the <see cref="IMo"/> interface, ignore all if not passed in.
        /// </summary>
        public Type[]? MoTypes { get; set; }
    }
}
