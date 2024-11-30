using System;

namespace Rougamo.Metadatas
{
    /// <summary>
    /// Define the lifetime of the type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class LifetimeAttribute(Lifetime lifetime) : Attribute
    {
        /// <summary>
        /// The lifetime of the type.
        /// </summary>
        public Lifetime Lifetime { get; } = lifetime;
    }
}
