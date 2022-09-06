using Rougamo.Context;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace Rougamo
{
    /// <summary>
    /// For methods whose return value is <see cref="Task"/>/<see cref="Task{TResult}"/>/<see cref="ValueTask"/>/<see cref="ValueTask{TResult}"/>,
    /// no matter whether async syntax is used or not, when using <see cref="MethodContext.ReplaceReturnValue(IMo, object)"/> to modify/set the
    /// return value or when using <see cref="MethodContext.HandledException(IMo, object)"/> to handle exceptions and set the return value, you
    /// can directly set the return value according to the return value type when using the async syntax.<br/>
    /// At the same time, the return value type in <see cref="ExMoAttribute"/> can be obtained through <see cref="MethodContext.ExReturnType"/>.<br/>
    /// For example:
    /// <code>
    /// // When using <see cref="MethodContext.ReplaceReturnValue(IMo, object)"/> to modify method M1's return value, the return value type should be string.
    /// // <see cref="MethodContext.ReturnType"/> = typeof(string)
    /// // <see cref="MethodContext.RealReturnType"/> = typeof(string)
    /// // <see cref="MethodContext.ExReturnType"/> = typeof(string)
    /// public string M1();
    /// 
    /// // When using <see cref="MethodContext.ReplaceReturnValue(IMo, object)"/> to modify method M2's return value, the return value type should be int.
    /// // <see cref="MethodContext.ReturnType"/> = typeof(Task&lt;int&gt;)
    /// // <see cref="MethodContext.RealReturnType"/> = typeof(string)
    /// // <see cref="MethodContext.ExReturnType"/> = typeof(string)
    /// public async Task&lt;int&gt; M2();
    /// 
    /// // When using <see cref="MethodContext.ReplaceReturnValue(IMo, object)"/> to modify method M3's return value, the return value type is also double.
    /// // This is different from <see cref="MoAttribute"/>, the return value type of M3 should be Task&lt;double&gt; when MoAttribute does not use async syntax.
    /// // <see cref="MethodContext.ReturnType"/> = typeof(Task&lt;double&gt;)
    /// // <see cref="MethodContext.RealReturnType"/> = typeof(Task&lt;double&gt;)
    /// // <see cref="MethodContext.ExReturnType"/> = typeof(string)
    /// public Task&lt;double&gt; M3();
    /// </code>
    /// </summary>
    public class ExMoAttribute : MoAttribute
    {
        private static readonly ConcurrentDictionary<Type, Func<object, ExMoAttribute, MethodContext, object>> _cache = new();

        static ExMoAttribute()
        {
            var commonTypes = new[] {
                typeof(int), typeof(long), typeof(float), typeof(double), typeof(byte), typeof(bool), typeof(string),
                typeof(object), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(bool?),
            };

            _cache.TryAdd(typeof(Task), TaskContinueWith);
            _cache.TryAdd(typeof(ValueTask), ValueTaskContinueWith);

            foreach (var type in commonTypes)
            {
                var taskType = typeof(Task<>).MakeGenericType(type);
                var valueTaskType = typeof(ValueTask<>).MakeGenericType(type);
                _cache.TryAdd(taskType, ResolveGenericTask(taskType));
                _cache.TryAdd(valueTaskType, ResolveGenericValueTask(valueTaskType));
            }
        }

        /// <summary>
        /// <inheritdoc cref="MoAttribute.OnEntry(MethodContext)"/>
        /// </summary>
        protected virtual void _OnEntry(MethodContext context) { }

        /// <summary>
        /// <inheritdoc cref="MoAttribute.OnException(MethodContext)"/>
        /// </summary>
        protected virtual void _OnException(MethodContext context) { }

        /// <summary>
        /// <inheritdoc cref="MoAttribute.OnSuccess(MethodContext)"/>
        /// </summary>
        protected virtual void _OnSuccess(MethodContext context) { }

        /// <summary>
        /// <inheritdoc cref="MoAttribute.OnExit(MethodContext)"/>
        /// </summary>
        protected virtual void _OnExit(MethodContext context) { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public sealed override void OnEntry(MethodContext context)
        {
            context.ExMode = true;
            _OnEntry(context);
            if (context.ReturnValueReplaced)
            {
                SetReturnValue(context);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public sealed override void OnException(MethodContext context)
        {
            _OnException(context);
            if (context.ExceptionHandled)
            {
                SetReturnValue(context);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public sealed override void OnSuccess(MethodContext context)
        {
            if (TryResolve(context.RealReturnType!, out var func))
            {
                try
                {
                    context.ExBarrier.AddParticipants(2);
                }
                catch
                {
                    // ignore
                }
                var returnValue = func!(context.ReturnValue!, this, context);
                context.ExMode = false;
                context.ReplaceReturnValue(this, returnValue);
                context.ExMode = true;
            }
            else
            {
                _OnSuccess(context);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public sealed override void OnExit(MethodContext context)
        {
            if(context.RealReturnType == context.ExReturnType)
            {
                _OnExit(context);
            }
            else
            {
                try
                {
                    context.ExBarrier.RemoveParticipant();
                }
                catch
                {
                    // ignore
                }
            }
        }

        private void SetReturnValue(MethodContext context)
        {
            if (!context.IsAsync)
            {
                if (typeof(Task) == context.RealReturnType)
                {
                    context.ReturnValue = Task.CompletedTask;
                }
                else if (typeof(Task).IsAssignableFrom(context.RealReturnType))
                {
                    context.ReturnValue = context.RealReturnType!.NewTaskResult(context.ReturnValue!);
                }
                else if (typeof(ValueTask) == context.RealReturnType)
                {
                    context.ReturnValue = default(ValueTask);
                }
                else if (context.RealReturnType!.FullName.StartsWith(Constants.FULLNAME_ValueTask))
                {
                    context.ReturnValue = context.RealReturnType!.NewValueTaskResult(context.ReturnValue!);
                }
            }
        }

        #region Resolve
        private static bool TryResolve(Type type, out Func<object, ExMoAttribute, MethodContext, object>? func)
        {
            if (typeof(Task) == type || typeof(ValueTask) == type)
            {
                _cache.TryGetValue(type, out func);
                return true;
            }
            if (typeof(Task).IsAssignableFrom(type))
            {
                func = _cache.GetOrAdd(type, ResolveGenericTask);
                return true;
            }
            if (type.FullName.StartsWith(Constants.FULLNAME_ValueTask))
            {
                func = _cache.GetOrAdd(type, ResolveGenericValueTask);
                return true;
            }

            func = null;
            return false;
        }

        private static Func<object, ExMoAttribute, MethodContext, object> ResolveGenericTask(Type type)
        {
            var methodInfo = typeof(ExMoAttribute).GetMethod(nameof(GenericTaskContinueWith), BindingFlags.NonPublic | BindingFlags.Static);
            var genericMethodInfo = methodInfo.MakeGenericMethod(type.GetGenericArguments()[0]);
            var @delegate = genericMethodInfo.CreateDelegate(typeof(Func<object, ExMoAttribute, MethodContext, object>));
            return (Func<object, ExMoAttribute, MethodContext, object>)@delegate;
        }

        private static Func<object, ExMoAttribute, MethodContext, object> ResolveGenericValueTask(Type type)
        {
            var methodInfo = typeof(ExMoAttribute).GetMethod(nameof(GenericValueTaskContinueWith), BindingFlags.NonPublic | BindingFlags.Static);
            var genericMethodInfo = methodInfo.MakeGenericMethod(type.GetGenericArguments()[0]);
            var @delegate = genericMethodInfo.CreateDelegate(typeof(Func<object, ExMoAttribute, MethodContext, object>));
            return (Func<object, ExMoAttribute, MethodContext, object>)@delegate;
        }
        #endregion Resolve

        #region ContinueWith
        private static object TaskContinueWith(object taskObject, ExMoAttribute mo, MethodContext context)
        {
            var tcs = new TaskCompletionSource<bool>();
            ((Task)taskObject).ContinueWith(t =>
            {
                try
                {
                    context.ExBarrier.SignalAndWait();
                }
                catch
                {
                    // ignore
                }
                if (t.IsFaulted)
                {
                    var exception = t.Exception.InnerExceptions.Count == 1 ? t.Exception.InnerException : t.Exception;
                    context.Exception = exception;
                    mo._OnException(context);
                    if (!context.ExceptionHandled)
                    {
                        mo._OnExit(context);
                        tcs.TrySetException(exception);
                        return;
                    }
                }
                else
                {
                    mo._OnSuccess(context);
                }
                mo._OnExit(context);
                tcs.TrySetResult(true);
            });

            return tcs.Task;
        }

        private static object ValueTaskContinueWith(object valueTaskObject, ExMoAttribute mo, MethodContext context)
        {
            var task = ((ValueTask)valueTaskObject).AsTask();
            task = (Task)TaskContinueWith(task, mo, context);
            return new ValueTask(task);
        }

        private static object GenericTaskContinueWith<T>(object taskObject, ExMoAttribute mo, MethodContext context)
        {
            var tcs = new TaskCompletionSource<T>();
            ((Task<T>)taskObject).ContinueWith(t =>
            {
                try
                {
                    context.ExBarrier.SignalAndWait();
                }
                catch
                {
                    // ignore
                }
                if (t.IsFaulted)
                {
                    var exception = t.Exception.InnerExceptions.Count == 1 ? t.Exception.InnerException : t.Exception;
                    context.Exception = exception;
                    mo._OnException(context);
                    mo._OnExit(context);
                    if (context.ExceptionHandled)
                    {
                        tcs.TrySetResult((T)context.ReturnValue!);
                    }
                    else
                    {
                        tcs.TrySetException(exception);
                    }
                }
                else
                {
                    mo._OnSuccess(context);
                    mo._OnExit(context);
                    tcs.TrySetResult((T)context.ReturnValue!);
                }
            });

            return tcs.Task;
        }

        private static object GenericValueTaskContinueWith<T>(object valueTaskObject, ExMoAttribute mo, MethodContext context)
        {
            var task = ((ValueTask<T>)valueTaskObject).AsTask();
            task = (Task<T>)GenericTaskContinueWith<T>(task, mo, context);
            return new ValueTask<T>(task);
        }
        #endregion ContinueWith
    }
}
