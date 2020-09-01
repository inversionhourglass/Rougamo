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
        public ExceptionContext(object target, MethodInfo method, params object[] args) : base(target, method, args) { }

        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception { get; set; }
    }
}
