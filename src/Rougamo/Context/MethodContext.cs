using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace Rougamo.Context
{
    /// <summary>
    /// Method execution context
    /// </summary>
    public class MethodContext
    {
        private readonly static IMo[] _EmptyMos = [];
        private readonly static object[] _EmptyArgs = [];
        
        private Type? _taskReturnType;
        private IDictionary? _datas;

        /// <summary>
        /// Compatibility with versions prior to 1.2.0
        /// </summary>
        [Obsolete("It will be deleted in version 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MethodContext(object target, Type targetType, MethodBase method, bool isAsync, bool isIterator, object[] args)
            : this(target, targetType, method, isAsync, isIterator, false, null, args) { }

        /// <summary>
        /// </summary>
        [Obsolete("It will be deleted in version 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MethodContext(object target, Type targetType, MethodBase method, bool isAsync, bool isIterator, bool mosNonEntryFIFO, IMo[]? mos, object[]? args)
        {
            Target = target;
            TargetType = targetType;
            Method = method;
            Mos = mos ?? _EmptyMos;
            Arguments = args ?? _EmptyArgs;
        }

        /// <summary>
        /// </summary>
        public MethodContext(object target, Type targetType, MethodBase method, IMo[]? mos, object[]? args)
        {
            Target = target;
            TargetType = targetType;
            Method = method;
            Mos = mos ?? _EmptyMos;
            Arguments = args ?? _EmptyArgs;
        }

        /// <summary>
        /// All of the <see cref="IMo"/> instances that apply to the current method.
        /// </summary>
        public IReadOnlyList<IMo> Mos { get; }

        /// <summary>
        /// User-defined state data.
        /// </summary>
        public IDictionary Datas => _datas ??= new Dictionary<object, object>();

        /// <summary>
        /// Instance of the declaring type; return null if the current method is static.
        /// </summary>
        public object? Target { get; private set; }

        /// <summary>
        /// The declaring type of current method.
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// Arguments of current method.
        /// </summary>
        public object?[] Arguments { get; private set; }

        /// <summary>
        /// Set it to true in OnEntry or OnEntryAsync; the <see cref="Arguments"/> you change in OnEntry/OnEntryAsync will rewrite the method's arguments.
        /// </summary>
        public bool RewriteArguments { get; set; }

        /// <summary>
        /// Current method information.
        /// </summary>
        public MethodBase Method { get; private set; }

        /// <summary>
        /// The return type of current method.
        /// </summary>
        public Type? ReturnType => (Method as MethodInfo)?.ReturnType;

        /// <summary>
        /// </summary>
        [Obsolete("Use TaskReturnType instead. It will be deleted in the next major version.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Type? RealReturnType => TaskReturnType;

        /// <summary>
        /// Get void if the method's return type is <see cref="Task"/> or <see cref="ValueTask"/>;
        /// get the first generic argument type if the method's return type is <see cref="Task{TResult}"/> or <see cref="ValueTask{TResult}"/>
        /// </summary>
        public Type? TaskReturnType
        {
            get
            {
                if(_taskReturnType == null && Method is MethodInfo methodInfo)
                {
                    var returnType = methodInfo.ReturnType;
                    if (returnType == typeof(Task) || returnType == typeof(ValueTask))
                    {
                        returnType = typeof(void);
                    }
                    else if (returnType.IsGenericType)
                    {
                        var definitionType = returnType.GetGenericTypeDefinition();
                        if (definitionType == typeof(Task<>) || definitionType == typeof(ValueTask<>))
                        {
                            returnType = returnType.GenericTypeArguments[0];
                        }
                    }
                    _taskReturnType = returnType;
                }
                return _taskReturnType;
            }
        }

        /// <summary>
        /// Return true if return value type is not void or Task or ValueTask
        /// </summary>
        public bool HasReturnValue => TaskReturnType != typeof(void);

        /// <summary>
        /// Method return value. Do not change it directly, call <see cref="ReplaceReturnValue(IMo, object)"/> or <see cref="HandledException(IMo, object)"/> instead.
        /// </summary>
        public object? ReturnValue { get; set; }

        /// <summary>
        /// Return true if return value has been replaced.
        /// </summary>
        public bool ReturnValueReplaced { get; private set; }

        /// <summary>
        /// Which <see cref="IMo"/> changed the return value.
        /// </summary>
        public IMo? ReturnValueModifier { get; private set; }

        /// <summary>
        /// Exception thrown by method. Do not change it directly, call <see cref="HandledException(IMo, object)"/> instead.
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Is there an unhandled exception.
        /// </summary>
        public bool HasException => Exception != null;

        /// <summary>
        /// Return true if the exception has been handled
        /// </summary>
        public bool ExceptionHandled { get; private set; }

        /// <summary>
        /// Which <see cref="IMo"/> handled the exception.
        /// </summary>
        public IMo? ExceptionHandler { get; private set; }

        /// <summary>
        /// The current method will re-execute if the value is greater than 0 after <see cref="IMo.OnSuccess(MethodContext)"/> and
        /// <see cref="IMo.OnException(MethodContext)"/> have been executed.
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Prevent exceptions thrown by the method and set the return value.
        /// If the return type is void, <paramref name="returnValue"/> is ignored.
        /// <see cref="ExceptionHandled"/> and <see cref="ReturnValueReplaced"/> will be set to true.
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
            ReturnValueReplaced = true;
            ReturnValueModifier = modifier;
            RetryCount = 0;
        }
    }
}
