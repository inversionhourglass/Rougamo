using Rougamo.Threading;
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
        private Type? _realReturnType;
        private Type? _exReturnType;
        private IDictionary? _datas;

        /// <summary>
        /// Compatibility with versions prior to 1.2.0
        /// </summary>
        [Obsolete]
        public MethodContext(object target, Type targetType, MethodBase method, bool isAsync, bool isIterator, object[] args)
            : this(target, targetType, method, isAsync, isIterator, false, new IMo[0], args) { }

        /// <summary>
        /// </summary>
        public MethodContext(object target, Type targetType, MethodBase method, bool isAsync, bool isIterator, bool mosNonEntryFIFO, IMo[] mos, object[] args)
        {
            Target = target;
            TargetType = targetType;
            Method = method;
            IsAsync = isAsync;
            IsIterator = isIterator;
            MosNonEntryFIFO = mosNonEntryFIFO;
            Mos = mos;
            Arguments = args;
        }

        /// <summary>
        /// When exmode is true, the return value type needs to match <see cref="ExReturnType"/>, otherwise it matches <see cref="RealReturnType"/>.
        /// </summary>
        internal bool ExMode { get; set; }

        internal SpinLocker ExLocker = new(3);

        /// <summary>
        /// ContinueWith executes only once.
        /// </summary>
        internal bool ExContinueOnce { get; set; }

        /// <summary>
        /// Array of IMos to which the current method applies.
        /// </summary>
        public IReadOnlyList<IMo> Mos { get; }

        /// <summary>
        /// Whether the execution order of multiple IMo non-OnEntry methods is consistent with OnEntry, the default false indicates that the execution order is opposite to OnEntry.
        /// </summary>
        public bool MosNonEntryFIFO { get; }

        /// <summary>
        /// user defined state data
        /// </summary>
        [Obsolete("The Dictionary type is more suitable for multi-developer scenarios, use Datas property instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object? Data { get; set; }

        /// <summary>
        /// user defined state datas
        /// </summary>
        public IDictionary Datas => _datas ??= new Dictionary<object, object>();

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
        /// When set to true, <see cref="Arguments"/> will rewrite method parameter values after <see cref="IMo.OnEntry(MethodContext)"/> is executed
        /// </summary>
        public bool RewriteArguments { get; set; }

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
                    else if (typeof(Task).IsAssignableFrom(returnType) || !returnType.IsGenericParameter && returnType.FullName != null && returnType.FullName.StartsWith(Constants.FULLNAME_ValueTask))
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
        /// The return value set by <see cref="ExMoAttribute"/>, the type of this value is equals to <see cref="ExReturnType"/>
        /// </summary>
        public object? ExReturnValue { get; set; }

        /// <summary>
        /// Is <see cref="ExReturnValue"/> has been set
        /// </summary>
        public bool ExReturnValueReplaced { get; private set; }

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
            var exReplace = ExMode && modifier is ExMoAttribute;
            ReplaceReturnValue(modifier, returnValue, exReplace);
        }

        /// <summary>
        /// Replace return value, if the return type is void, <paramref name="returnValue"/> is ignored.
        /// <see cref="ReturnValueReplaced"/> will be set to true
        /// </summary>
        internal void ReplaceReturnValue(IMo modifier, object returnValue, bool exMode)
        {
            if (HasReturnValue)
            {
                if (exMode)        
                {
                    ExReturnValue = returnValue;
                }
                else
                {
                    ReturnValue = returnValue;
                }
            }
            if (exMode)
            {
                ExReturnValueReplaced = true;
            }
            else
            {
                ReturnValueReplaced = true;
            }
            ReturnValueModifier = modifier;
            RetryCount = 0;
        }
    }
}
