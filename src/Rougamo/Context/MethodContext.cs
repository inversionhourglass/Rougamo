using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        
        private Type? _realReturnType;
        private IDictionary? _datas;

        /// <summary>
        /// Compatibility with versions prior to 1.2.0
        /// </summary>
        [Obsolete]
        public MethodContext(object target, Type targetType, MethodBase method, bool isAsync, bool isIterator, object[] args)
            : this(target, targetType, method, isAsync, isIterator, false, null, args) { }

        /// <summary>
        /// </summary>
        [Obsolete]
        public MethodContext(object target, Type targetType, MethodBase method, bool isAsync, bool isIterator, bool mosNonEntryFIFO, IMo[]? mos, object[]? args)
        {
            Target = target;
            TargetType = targetType;
            Method = method;
            IsAsync = isAsync;
            IsIterator = isIterator;
            MosNonEntryFIFO = mosNonEntryFIFO;
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
            IsAsync = true;
            IsIterator = false;
            MosNonEntryFIFO = false;
            Mos = mos ?? _EmptyMos;
            Arguments = args ?? _EmptyArgs;
        }

        /// <summary>
        /// Array of IMos to which the current method applies.
        /// </summary>
        public IReadOnlyList<IMo> Mos { get; }

        /// <summary>
        /// Whether the execution order of multiple IMo non-OnEntry methods is consistent with OnEntry, the default false indicates that the execution order is opposite to OnEntry.
        /// </summary>
        public bool MosNonEntryFIFO { get; }

        /// <summary>
        /// User defined state data
        /// </summary>
        [Obsolete("The Dictionary type is more suitable for multi-developer scenarios, use Datas property instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object? Data { get; set; }

        /// <summary>
        /// User defined state datas
        /// </summary>
        public IDictionary Datas => _datas ??= new Dictionary<object, object>();

        /// <summary>
        /// Instance of declaring type, return null if current method is static
        /// </summary>
        public object? Target { get; private set; }

        /// <summary>
        /// Declaring type of current method
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// Arguments of current method
        /// </summary>
        public object?[] Arguments { get; private set; }

        /// <summary>
        /// When set to true, <see cref="Arguments"/> will rewrite method parameter values after <see cref="IMo.OnEntry(MethodContext)"/> is executed
        /// </summary>
        public bool RewriteArguments { get; set; }

        /// <summary>
        /// Method info
        /// </summary>
        public MethodBase Method { get; private set; }

        /// <summary>
        /// Return true if method use async/await syntax
        /// </summary>
        public bool IsAsync { get; }

        /// <summary>
        /// Return true if method use yield return syntax
        /// </summary>
        public bool IsIterator { get; }

        /// <summary>
        /// Method decleard return type, return <see cref="Task"/>/<see cref="Task{TResult}"/>/ValueTask/ValueTask&lt;TResult&gt; even if method run in async
        /// </summary>
        public Type? ReturnType => (Method as MethodInfo)?.ReturnType;

        /// <summary>
        /// Method real return type, return first generic argument if current method is an async <see cref="Task{TResult}"/>/ValueTask&lt;TResult&gt; method, 
        /// or return typeof(void) if method is an async <see cref="Task"/>/ValueTask method
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
                        if (returnType == typeof(void) || returnType == typeof(Task) || returnType.FullName == Constants.FULLNAME_ValueTask)
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
        /// Return true if return value type is not void
        /// </summary>
        public bool HasReturnValue => RealReturnType != typeof(void);

        /// <summary>
        /// Method return value, if you want to assign a value to it, you'd better use <see cref="HandledException(IMo, object)"/> or <see cref="ReplaceReturnValue(IMo, object)"/>, 
        /// the type of this value is equals to <see cref="RealReturnType"/>
        /// </summary>
        public object? ReturnValue { get; set; }

        /// <summary>
        /// Return true if return value has been replaced
        /// </summary>
        public bool ReturnValueReplaced { get; private set; }

        /// <summary>
        /// When multiple <see cref="IMo"/> applied to the method, you will know who replace the return value
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
        /// This property will be checked after <see cref="MoAttribute.OnSuccess(MethodContext)"/> and <see cref="MoAttribute.OnException(MethodContext)"/>,
        /// and the method will be re-executed when the value is greater than 0 (skipping the OnEntry method execution).
        /// </summary>
        public int RetryCount { get; set; }

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
            ReturnValueReplaced = true;
            ReturnValueModifier = modifier;
            RetryCount = 0;
        }
    }
}
