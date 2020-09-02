using System;
using System.Reflection;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行成功（无异常）上下文
    /// </summary>
    public sealed class SuccessContext : MethodContext
    {
        /// <summary>
        /// </summary>
        public SuccessContext(bool hasReturnValue, object returnValue, object target, Type targetType, MethodBase method, params object[] args) : base(target, targetType, method, args)
        {
            HasReturnValue = hasReturnValue;
            ReturnValue = returnValue;
        }

        /// <summary>
        /// 方法返回值，不可修改
        /// </summary>
        public object ReturnValue { get; }

        /// <summary>
        /// 是否有返回值
        /// </summary>
        public bool HasReturnValue { get; }
    }
}
