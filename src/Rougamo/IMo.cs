using Rougamo.Context;

namespace Rougamo
{
    /// <summary>
    /// </summary>
    public interface IMo
    {
        /// <summary>
        /// Weavable method type, this attribute has no effect when applied at the method level.
        /// </summary>
        AccessFlags Flags { get; }

        /// <summary>
        /// Before the method executing.
        /// </summary>
        void OnEntry(MethodContext context);

        /// <summary>
        /// After the method executes successfully
        /// </summary>
        void OnSuccess(MethodContext context);

        /// <summary>
        /// When an exception occurs when the method is executed.
        /// </summary>
        void OnException(MethodContext context);

        /// <summary>
        /// After the method is executed, whether it succeeds or an exception occurs.
        /// </summary>
        void OnExit(MethodContext context);
    }
}
