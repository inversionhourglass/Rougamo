using Rougamo.Context;
using System;

namespace Rougamo.Metadatas
{
    /// <summary>
    /// Rougamo optimize attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class OptimizationAttribute : Attribute
    {
        /// <summary>
        /// Which members of MethodContext you don't need
        /// </summary>
        public Omit MethodContext { get; set; }

        /// <summary>
        /// Which methods should be executed synchronously in an async method
        /// </summary>
        public ForceSync ForceSync { get; set; }
    }
}
