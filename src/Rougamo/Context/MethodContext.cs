using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rougamo.Context
{
    /// <summary>
    /// Method execution context
    /// </summary>
    public sealed class MethodContext
    {
        private Type? _realReturnType;
        private Type? _exReturnType;

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
            Datas = new Dictionary<object, object>();
        }

        /// <summary>
        /// user defined state data
        /// </summary>
        [Obsolete("The Dictionary type is more suitable for multi-developer scenarios, use Datas property instead")]
        public object? Data { get; set; }

        /// <summary>
        /// user defined state datas
        /// </summary>
        public IDictionary Datas { get; }

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
        /// Method decleard return type, return <see cref="Task"/>/<see cref="Task{TResult}"/>/<see cref="ValueTask"/>/<see cref="ValueTask{TResult}"/> even if method run in async
        /// </summary>
        [Obsolete("use RealReturnType to instead")]
        public Type? ReturnType => (Method as MethodInfo)?.ReturnType;

        /// <summary>
        /// Method real return type, return first generic argument if current method is an async <see cref="Task{TResult}"/>/<see cref="ValueTask{TResult}"/> method, 
        /// or return typeof(void) if method is an async <see cref="Task"/>/<see cref="ValueTask"/> method
        /// </summary>
        public Type? RealReturnType
        {
            get
            {
                if(_realReturnType == null && Method is MethodInfo methodInfo)
                {
                    var returnType = methodInfo.ReturnType;
                    if (IsAsync)
                    {
                        if (returnType == typeof(void) || returnType == typeof(Task) || returnType == typeof(ValueTask))
                        {
                            _realReturnType = typeof(void);
                        }
                        else
                        {
                            _realReturnType = returnType.GetGenericArguments().Single();
                        }
                    }
                    else
                    {
                        _realReturnType = returnType;
                    }
                }
                return _realReturnType;
            }
        }

        /// <summary>
        /// <see cref="Task"/> and <see cref="ValueTask"/> return typeof(void), and <see cref="Task{TResult}"/> and <see cref="ValueTask{TResult}"/> return typeof(T),
        /// whether or not the async syntax is used. Others return the actual method return value type.
        /// </summary>
        public Type? ExReturnType
        {
            get
            {
                if (_exReturnType == null && Method is MethodInfo methodInfo)
                {
                    var returnType = methodInfo.ReturnType;
                    if (returnType == typeof(Task) || returnType == typeof(ValueTask))
                    {
                        _exReturnType = typeof(void);
                    }
                    else if (typeof(Task).IsAssignableFrom(returnType) || returnType.FullName.StartsWith(Constants.FULLNAME_ValueTask))
                    {
                        _exReturnType = returnType.GetGenericArguments().Single();
                    }
                    else
                    {
                        _exReturnType = returnType;
                    }
                }
                return _exReturnType;
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
                ReturnValue = returnValue;
            }
            ReturnValueModifier = modifier;
            ReturnValueReplaced = true;
        }
    }
}
