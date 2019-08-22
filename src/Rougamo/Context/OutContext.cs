using System;
using System.Reflection;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行完成上下文
    /// </summary>
    public sealed class OutContext : MethodContext
    {
        private object _actuallyReturn = UNSET;

        /// <summary>
        /// </summary>
        public OutContext(object target, MethodInfo method, params object[] args) : base(target, method, args) { }

        /// <summary>
        /// 原方法执行是否触发异常
        /// </summary>
        public bool ExceptionOccurred => OriginException != null;

        /// <summary>
        /// 原方法触发的异常是否已处理
        /// </summary>
        public bool ExceptionHandled { get; set; }

        /// <summary>
        /// 原方法抛出的异常
        /// </summary>
        public Exception OriginException { get; set; }

        /// <summary>
        /// 手动引发的异常
        /// </summary>
        public Exception Throw { get; set; }

        /// <summary>
        /// 是否替换了原始方法返回值
        /// </summary>
        public bool ReturnReplaced { get; set; }

        /// <summary>
        /// 原始返回值
        /// </summary>
        public object OriginReturn { get; set; } = UNSET;

        /// <summary>
        /// 实际返回值
        /// </summary>
        public object ActuallyReturn
        {
            get => _actuallyReturn == UNSET ? OriginReturn : _actuallyReturn;
            set => _actuallyReturn = value;
        }
    }
}
