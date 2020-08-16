using System;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行异常上下文
    /// </summary>
    public sealed class ExceptionContext : MethodContext
    {
        private readonly ExitContext _exit;

        /// <summary>
        /// </summary>
        public ExceptionContext(ExitContext exit, Exception exception) : base(exit.Target, exit.Method, exit.Arguments)
        {
            _exit = exit;
            Exception = _exit.Exception = exception;
        }

        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception { get; }
    }
}
