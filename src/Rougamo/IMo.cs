using Rougamo.Context;

namespace Rougamo
{
    /// <summary>
    /// </summary>
    public interface IMo
    {
        /// <summary>
        /// 可织入方法类型，应用于方法级别时，该属性无效
        /// </summary>
        AccessFlags Flags { get; }

        /// <summary>
        /// 方法执行前切入执行
        /// </summary>
        void OnEntry(MethodContext context);

        /// <summary>
        /// 方法执行成功后切入执行
        /// </summary>
        void OnSuccess(MethodContext context);

        /// <summary>
        /// 方法执行引发异常后切入执行
        /// </summary>
        void OnException(MethodContext context);

        /// <summary>
        /// 方法执行完成后切入执行
        /// </summary>
        void OnExit(MethodContext context);
    }
}
