using System;

namespace Rougamo.Context
{
    /// <summary>
    /// To minimize boxing operations, discard some useless objects is the best way.
    /// </summary>
    [Flags]
#if ROUGAMO
    public enum Omit : int
#else
    internal enum Omit : int
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
        /// Omit MethodContext.ReturnValue property
        /// </summary>
        ReturnValue = 0x4,
        /// <summary>
        /// Omit all
        /// </summary>
        All = Mos | Arguments | ReturnValue
    }
}
