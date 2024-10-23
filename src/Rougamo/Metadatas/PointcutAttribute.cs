using System;

namespace Rougamo.Metadatas
{
    /// <summary>
    /// Rougamo pointcut attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PointcutAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        public PointcutAttribute(AccessFlags accessFlags)
        {
            Flags = accessFlags;
        }

        /// <summary>
        /// </summary>
        public PointcutAttribute(string pattern)
        {
            Pattern = pattern;
            Flags = AccessFlags.InstancePublic;
        }

        /// <summary>
        /// Modifier flags
        /// </summary>
        public AccessFlags Flags { get; }

        /// <summary>
        /// AspectN pattern
        /// </summary>
        /// <remarks>
        /// https://github.com/inversionhourglass/Shared.Cecil.AspectN
        /// </remarks>
        public string? Pattern { get; }
    }
}
