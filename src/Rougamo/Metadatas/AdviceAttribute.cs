using System;

namespace Rougamo.Metadatas
{
    /// <summary>
    /// Rougamo advice attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class AdviceAttribute(Feature features) : Attribute
    {
        /// <summary>
        /// Which features you need
        /// </summary>
        public Feature Features => features;
    }
}
