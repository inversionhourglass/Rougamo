using System;

#if FODY
namespace Rougamo.Fody
#else
namespace Rougamo.Context
#endif
{
    /// <summary>
    /// To minimize boxing operations, discard some useless objects is the best way.
    /// </summary>
    [Flags]
#if FODY
    internal enum Omit : int
#else
    public enum Omit : int
#endif
    {
        /// <summary>
        /// Do not omit anything
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Omit MethodContext.Mos property
        /// </summary>
        Mos = 0x1,
        /// <summary>
        /// Omit MethodContext.Arguments property
        /// </summary>
        Arguments = 0x2,
        /// <summary>
        /// Omit all
        /// </summary>
        All = Mos | Arguments
    }
}
