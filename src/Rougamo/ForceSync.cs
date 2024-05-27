using System;

namespace Rougamo
{
    /// <summary>
    /// Which methods should be executed synchronously in an async method
    /// </summary>
    [Flags]
#if FODY
    internal enum ForceSync : int
#else
    public enum ForceSync : int
#endif
    {
        /// <summary>
        /// None of mo's methods should be executed synchronously in async method
        /// </summary>
        None = 0x0,
        /// <summary>
        /// OnEntry should be executed synchronously in async method
        /// </summary>
        OnEntry = 0x1,
        /// <summary>
        /// OnSuccess should be executed synchronously in async method
        /// </summary>
        OnSuccess = 0x2,
        /// <summary>
        /// OnException should be executed synchronously in async method
        /// </summary>
        OnException = 0x4,
        /// <summary>
        /// OnExit should be executed synchronously in async method
        /// </summary>
        OnExit = 0x8,
        /// <summary>
        /// All of mo's methods should be executed synchronously in async method
        /// </summary>
        All = OnEntry | OnSuccess | OnException | OnExit
    }
}
