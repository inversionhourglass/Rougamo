using System.Reflection;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行成功（无异常）上下文
    /// </summary>
    public sealed class SuccessContext : MethodContext
    {
        private object _returnValue;

        /// <summary>
        /// </summary>
        public SuccessContext(object target, MethodInfo method, params object[] args) : base(target, method, args) { }

        /// <summary>
        /// 方法返回值
        /// </summary>
        public object ReturnValue
        {
            get => _returnValue;
            set
            {
                HasReturnValue = true;
                _returnValue = value;
            }
        }

        /// <summary>
        /// 是否有返回值
        /// </summary>
        public bool HasReturnValue { get; private set; }
    }
}
