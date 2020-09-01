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
        public ExitContext(object target, MethodInfo method, params object[] args) : base(target, method, args) { }
    }
}
