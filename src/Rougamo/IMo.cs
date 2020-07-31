using Rougamo.Context;

namespace Rougamo
{
    /// <summary>
    /// </summary>
    public interface IMo
    {
        /// <summary>
        /// 方法执行前切入执行
        /// </summary>
        void OnEntry(EntryContext context);

        /// <summary>
        /// 方法执行成功后切入执行
        /// </summary>
        void OnSuccess(SuccessContext context);

        /// <summary>
        /// 方法执行引发异常后切入执行
        /// </summary>
        void OnException(ExceptionContext context);

        /// <summary>
        /// 方法执行完成后切入执行
        /// </summary>
        void OnExit(ExitContext context);
    }
}
