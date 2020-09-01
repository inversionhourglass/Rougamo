using System.Reflection;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行前上线文
    /// </summary>
    public sealed class EntryContext : MethodContext
    {
        /// <summary>
        /// </summary>
        public EntryContext(object target, MethodInfo method, params object[] args) : base(target, method, args) { }
    }
}
