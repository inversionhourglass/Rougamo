﻿using System.Reflection;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行上下文共享部分
    /// </summary>
    public abstract class MethodContext
    {
        /// <summary>
        /// </summary>
        protected MethodContext(object target, MethodInfo method, params object[] args)
        {
            Target = target;
            Method = method;
            Arguments = args;
        }

        /// <summary>
        /// 宿主类实例
        /// </summary>
        public object Target { get; private set; }

        /// <summary>
        /// 方法入参
        /// </summary>
        public object[] Arguments { get; private set; }

        /// <summary>
        /// 切入方法
        /// </summary>
        public MethodInfo Method { get; private set; }
    }
}
