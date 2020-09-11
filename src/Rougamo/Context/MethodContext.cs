using System;
using System.Reflection;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行上下文共享部分
    /// </summary>
    public sealed class MethodContext
    {
        private Exception _exception;
        private object _returnValue;
        private bool _returnValueSet;

        /// <summary>
        /// </summary>
        public MethodContext(object target, Type targetType, MethodBase method, object[] args)
        {
            Target = target;
            TargetType = targetType;
            Method = method;
            Arguments = args;
        }

        /// <summary>
        /// 宿主类实例
        /// </summary>
        public object Target { get; private set; }

        /// <summary>
        /// 宿主类型
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// 方法入参
        /// </summary>
        public object[] Arguments { get; private set; }

        /// <summary>
        /// 切入方法
        /// </summary>
        public MethodBase Method { get; private set; }

        /// <summary>
        /// 方法返回类型
        /// </summary>
        public Type ReturnType => (Method as MethodInfo)?.ReturnType;

        /// <summary>
        /// 是否有返回值（非void）
        /// </summary>
        public bool HasReturnValue => ReturnType != typeof(void);

        /// <summary>
        /// 异常，不可修改，静态织入时设置
        /// </summary>
        public Exception Exception
        {
            get => _exception;
            set
            {
                if (_returnValueSet) return;
                _returnValueSet = true;
                _exception = value;
            }
        }

        /// <summary>
        /// 是否有异常
        /// </summary>
        public bool HasException => Exception != null;

        /// <summary>
        /// 方法返回值，不可修改，静态织入时设置
        /// </summary>
        public object ReturnValue
        {
            get => _returnValue;
            set
            {
                if (_returnValueSet) return;
                _returnValueSet = true;
                _returnValue = value;
            }
        }
    }
}
