using System;

namespace Rougamo
{
    /// <summary>
    /// Multiple mutex types, used with <see cref="IRepulsionsRougamo{TMo, TRepulsion}"/>.
    /// </summary>
    public abstract class MoRepulsion
    {
        /// <summary>
        /// The type must implements <see cref="IMo"/>, and when implementing this class,
        /// the field must be initialized once and cannot contain logic processing.
        /// </summary>
        /// <example>
        /// Repulsions = new [] { typeof(Abc), typeof(Bcd) };
        /// </example>
        public abstract Type[] Repulsions { get; }
    }
}
