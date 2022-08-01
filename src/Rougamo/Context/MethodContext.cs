using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rougamo.Context
{
    /// <summary>
    /// 方法执行上下文共享部分
    /// </summary>
    public sealed class MethodContext
    {
        /// <summary>
        /// </summary>
        public MethodContext(object target, Type targetType, MethodBase method, bool isAsync, bool isIterator, object[] args)
        {
            Target = target;
            TargetType = targetType;
            Method = method;
            IsAsync = isAsync;
            IsIterator = isIterator;
            Arguments = args;
        }

        /// <summary>
        /// user defined state data
        /// </summary>
        public object? Data { get; set; }

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
        /// Is method run in async
        /// </summary>
        public bool IsAsync { get; }

        /// <summary>
        /// return true if method use yield return syntax
        /// </summary>
        public bool IsIterator { get; }

        /// <summary>
        /// Method decleard return type, return Task/Task&lt;&gt;/ValueTask/ValueTask&lt;&gt; even if method run in async
        /// </summary>
        [Obsolete("use RealReturnType to instead")]
        public Type? ReturnType => (Method as MethodInfo)?.ReturnType;

        /// <summary>
        /// Method real return type, return first generic argument if current method is an async Task&lt;&gt;/ValueTask&lt;&gt; method, 
        /// or return typeof(void) if method is an async Task/ValueTask method
        /// </summary>
        public Type? RealReturnType
        {
            get
            {
                if(Method is MethodInfo methodInfo)
                {
                    var returnType = methodInfo.ReturnType;
                    if (IsAsync)
                    {
                        if (returnType == typeof(void) || returnType == typeof(Task) || returnType.FullName == "System.Threading.Tasks.ValueTask")
                        {
                            return typeof(void);
                        }
                        return returnType.GetGenericArguments().Single();
                    }
                    return returnType;
                }
                return null;
            }
        }

        /// <summary>
        /// Return true if return value type is not void
        /// </summary>
        public bool HasReturnValue => RealReturnType != typeof(void);

        /// <summary>
        /// Method return value, if you want to assign a value to it, you'd better use <see cref="HandledException(IMo, object)"/> or <see cref="ReplaceReturnValue(IMo, object)"/>
        /// </summary>
        public object? ReturnValue { get; set; }

        /// <summary>
        /// Return true if return value has been replaced
        /// </summary>
        public bool ReturnValueReplaced { get; private set; }

        /// <summary>
        /// when multiple <see cref="IMo"/> applied to the method, you will know who replace the return value
        /// </summary>
        public IMo? ReturnValueModifier { get; private set; }

        /// <summary>
        /// Exception throws by method, if you want to prevent exception, you'd better use <see cref="HandledException(IMo, object)"/>
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Is there a unhandled exception
        /// </summary>
        public bool HasException => Exception != null;

        /// <summary>
        /// Return true if exception has been handled
        /// </summary>
        public bool ExceptionHandled { get; private set; }

        /// <summary>
        /// when multiple <see cref="IMo"/> applied to the method, you will know who handled the exception
        /// </summary>
        public IMo? ExceptionHandler { get; private set; }

        /// <summary>
        /// Prevent exception thrown by the method and set the return value.
        /// If the return type is void, <paramref name="returnValue"/> is ignored.
        /// <see cref="ExceptionHandled"/> and <see cref="ReturnValueReplaced"/> will be set to true
        /// </summary>
        public void HandledException(IMo handler, object returnValue)
        {
            ReplaceReturnValue(handler, returnValue);
            ExceptionHandler = handler;
            ExceptionHandled = true;
            Exception = null;
        }

        /// <summary>
        /// Replace return value, if the return type is void, <paramref name="returnValue"/> is ignored.
        /// <see cref="ReturnValueReplaced"/> will be set to true
        /// </summary>
        public void ReplaceReturnValue(IMo modifier, object returnValue)
        {
            if (HasReturnValue)
            {
                if (RealReturnType == null ||
                    returnValue == null && RealReturnType.IsValueType && (!RealReturnType.IsGenericType || RealReturnType.GetGenericTypeDefinition() != typeof(Nullable<>)) ||
                    returnValue != null && !RealReturnType.IsAssignableFrom(returnValue.GetType()))
                {
                    throw new ArgumentException($"Method return type({RealReturnType?.FullName}) is not assignable from returnvalue({returnValue?.GetType().FullName})");
                }
                ReturnValue = returnValue;
            }
            ReturnValueModifier = modifier;
            ReturnValueReplaced = true;
        }
    }
}
