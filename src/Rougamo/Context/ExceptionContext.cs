using System;
using System.Reflection;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行异常上下文
    /// </summary>
    public sealed class ExceptionContext : MethodContext
    {
        /// <summary>
        /// </summary>
        public ExceptionContext(Exception exception, object target, Type targetType, MethodBase method, params object[] args) : base(target, targetType, method, args)
        {
            Exception = exception;
        }

        /// <summary>
        /// 异常，不可修改
        /// </summary>
        public Exception Exception { get; }
    }
}
