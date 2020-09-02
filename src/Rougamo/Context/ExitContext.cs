using System;
using System.Reflection;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行完成上下文
    /// </summary>
    public sealed class ExitContext : MethodContext
    {
        /// <summary>
        /// </summary>
        public ExitContext(object target, Type targetType, MethodBase method, params object[] args) : base(target, targetType, method, args) { }
    }
}
