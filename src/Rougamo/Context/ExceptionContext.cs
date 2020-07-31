using System;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行异常上下文
    /// </summary>
    public sealed class ExceptionContext : MethodContext
    {
        private readonly ExitContext _exit;
        private Exception _exception;

        /// <summary>
        /// </summary>
        public ExceptionContext(ExitContext exit) : base(exit.Target, exit.Method, exit.Arguments)
        {
            _exit = exit;
        }

        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception
        {
            get => _exception;
            // todo: change to private
            set
            {
                _exception = value;
                _exit.Exception = value;
            }
        }
    }
}
