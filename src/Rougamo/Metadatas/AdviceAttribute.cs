using System;

namespace Rougamo.Metadatas
{
    /// <summary>
    /// Rougamo advice attribute
    /// </summary>
    public sealed class AdviceAttribute(Feature features) : Attribute
    {
        /// <summary>
        /// Which features you need
        /// </summary>
        public Feature Features => features;
    }
}
