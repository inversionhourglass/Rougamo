using System;
using System.Reflection;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行上下文共享部分
    /// </summary>
    public sealed class MethodContext
    {
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
        /// Method' declaring type instance, null if method is a static method
        /// </summary>
        public object Target { get; private set; }

        /// <summary>
        /// Method' declaring type
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// Method arguments
        /// </summary>
        public object[] Arguments { get; private set; }

        /// <summary>
        /// Method info
        /// </summary>
        public MethodBase Method { get; private set; }

        /// <summary>
        /// Method return type
        /// </summary>
        public Type ReturnType => (Method as MethodInfo)?.ReturnType;

        /// <summary>
        /// Return true if return value type is not void
        /// </summary>
        public bool HasReturnValue => ReturnType != typeof(void);

        /// <summary>
        /// Method return value, if you want to assign a value to it, you'd better use <see cref="HandledException(object)"/> or <see cref="ReplaceReturnValue(object)"/>
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// Return true if return value has been replaced
        /// </summary>
        public bool ReturnValueReplaced { get; private set; }

        /// <summary>
        /// Exception throws by method, if you want to prevent exception, you'd better use <see cref="HandledException(object)"/>
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Is there a unhandled exception
        /// </summary>
        public bool HasException => Exception != null;

        /// <summary>
        /// Return true if exception has been handled
        /// </summary>
        public bool ExceptionHandled { get; private set; }

        /// <summary>
        /// Prevent exception thrown by the method and set the return value.
        /// If the return type is void, <paramref name="returnValue"/> is ignored.
        /// <see cref="ExceptionHandled"/> and <see cref="ReturnValueReplaced"/> will be set to true
        /// </summary>
        public void HandledException(object returnValue)
        {
            ReplaceReturnValue(returnValue);
            ExceptionHandled = true;
            Exception = null;
        }

        /// <summary>
        /// Replace return value, if the return type is void, <paramref name="returnValue"/> is ignored.
        /// <see cref="ReturnValueReplaced"/> will be set to true
        /// </summary>
        /// <param name="returnValue"></param>
        /// <exception cref="ArgumentException"></exception>
        public void ReplaceReturnValue(object returnValue)
        {
            if (HasReturnValue)
            {
                if (!ReturnType.IsAssignableFrom(returnValue.GetType()))
                {
                    throw new ArgumentException($"Method return type({ReturnType.FullName}) is not assignable from returnvalue({returnValue.GetType().FullName})");
                }
                ReturnValue = returnValue;
            }
            ReturnValueReplaced = true;
        }
    }
}
