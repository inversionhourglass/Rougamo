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
    public class MethodContext : IResettable
    {
        private Type? _taskReturnType;
        private IDictionary? _datas;
        private MethodBase _method = null!;
        private bool _methodResolved;

        /// <summary>
        /// User-defined state data.
        /// </summary>
        public IDictionary Datas => _datas ??= new Dictionary<object, object>();

        /// <summary>
        /// All of the <see cref="IMo"/> instances that apply to the current method.
        /// </summary>
        public IReadOnlyList<IMo> Mos { get; set; } = [];

        /// <summary>
        /// Instance of the declaring type; return null if the current method is static.
        /// </summary>
        public object? Target { get; set; }

        /// <summary>
        /// The declaring type of current method.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Type TargetType { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        /// <summary>
        /// Current method information.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public MethodBase Method
        {
            get
            {
                if (!_methodResolved)
                {
                    _method = ResolveMethod(_method);
                    _methodResolved = true;
                }
                return _method;
            }
            set
            {
                _method = value;
                _methodResolved = false;
            }
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        /// <summary>
        /// Arguments of current method.
        /// </summary>
        public object?[] Arguments { get; set; } = [];

        /// <summary>
        /// Set it to true in OnEntry or OnEntryAsync; the <see cref="Arguments"/> you change in OnEntry/OnEntryAsync will rewrite the method's arguments.
        /// </summary>
        public bool RewriteArguments { get; set; }

        /// <summary>
        /// The return type of current method.
        /// </summary>
        public Type? ReturnType => (Method as MethodInfo)?.ReturnType;

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

        /// <inheritdoc/>
        public bool TryReset()
        {
            _taskReturnType = null;
            _datas = null;
            Mos = [];
            Target = null;
            TargetType = null!;
            Method = null!;
            Arguments = [];
            RetryCount = 0;
            RewriteArguments = false;
            ReturnValue = null;
            ReturnValueReplaced = false;
            ReturnValueModifier = null;
            Exception = null;
            ExceptionHandled = false;
            ExceptionHandler = null;

            return true;
        }

        private static MethodBase ResolveMethod(MethodBase method)
        {
            if (method == null) return null!;

            method = ResolveStateMachineMethod(method);
            method = ResolveRougamoProxyMethod(method);
            return method;
        }

        private static MethodBase ResolveStateMachineMethod(MethodBase method)
        {
            if (method.Name != "MoveNext") return method;

            var stateMachineType = method.DeclaringType;
            var declaringType = stateMachineType?.DeclaringType;
            if (stateMachineType == null || declaringType == null) return method;

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            foreach (var candidate in declaringType.GetMethods(flags))
            {
                var attributes = candidate.GetCustomAttributes(inherit: false);
                foreach (var attribute in attributes)
                {
                    var attrType = attribute.GetType();
                    var fullName = attrType.FullName;
                    if (fullName != "System.Runtime.CompilerServices.AsyncStateMachineAttribute" &&
                        fullName != "System.Runtime.CompilerServices.IteratorStateMachineAttribute" &&
                        fullName != "System.Runtime.CompilerServices.AsyncIteratorStateMachineAttribute")
                    {
                        continue;
                    }

                    var stateMachineTypeProperty = attrType.GetProperty("StateMachineType", BindingFlags.Public | BindingFlags.Instance);
                    if (stateMachineTypeProperty?.GetValue(attribute) is Type candidateStateMachineType &&
                        candidateStateMachineType == stateMachineType)
                    {
                        return candidate;
                    }
                }
            }

            return method;
        }

        private static MethodBase ResolveRougamoProxyMethod(MethodBase method)
        {
            const string prefix = "$Rougamo_";
            if (!method.Name.StartsWith(prefix, StringComparison.Ordinal)) return method;

            var declaringType = method.DeclaringType;
            if (declaringType == null) return method;

            var expectedName = method.Name.Substring(prefix.Length);
            var methodGenericCount = method.IsGenericMethod ? method.GetGenericArguments().Length : 0;
            var parameterCount = method.GetParameters().Length;

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var candidates = declaringType.GetMethods(flags)
                .Where(m => m.Name == expectedName)
                .Where(m => m.GetParameters().Length == parameterCount)
                .ToArray();

            foreach (var candidate in candidates)
            {
                var candidateGenericCount = candidate.IsGenericMethod ? candidate.GetGenericArguments().Length : 0;
                if (candidateGenericCount == methodGenericCount)
                {
                    return candidate;
                }
            }

            return candidates.FirstOrDefault() ?? method;
        }
    }
}
