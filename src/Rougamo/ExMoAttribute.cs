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
        //private const string EX_MOS_DATA_KEY = "rougamo.ex.mos";
        private const string EX_SYNC_EXCEPTION = "rougamo.ex.syncException";
        //private const string REVERSE_CALL_KEY = "rougamo.reverse";
        private static readonly ConcurrentDictionary<Type, Func<object, MethodContext, object>> _cache = new();

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
        protected virtual void ExOnEntry(MethodContext context) { }

        /// <summary>
        /// <inheritdoc cref="MoAttribute.OnException(MethodContext)"/>
        /// </summary>
        protected virtual void ExOnException(MethodContext context) { }

        /// <summary>
        /// <inheritdoc cref="MoAttribute.OnSuccess(MethodContext)"/>
        /// </summary>
        protected virtual void ExOnSuccess(MethodContext context) { }

        /// <summary>
        /// <inheritdoc cref="MoAttribute.OnExit(MethodContext)"/>
        /// </summary>
        protected virtual void ExOnExit(MethodContext context) { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public sealed override void OnEntry(MethodContext context)
        {
            context.ExMode = true;
            ExOnEntry(context);
            if (context.ExReturnValueReplaced)
            {
                SetReturnValue(context);
            }
            context.ExMode = false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public sealed override void OnException(MethodContext context)
        {
            context.ExMode = true;
            context.Datas[EX_SYNC_EXCEPTION] = true;
            ExOnException(context);
            if (context.ExceptionHandled)
            {
                SetReturnValue(context);
            }
            context.ExMode = false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public sealed override void OnSuccess(MethodContext context)
        {
            context.ExMode = true;
            // todo: 对于泛型Task<>/ValueTask<>返回值，目前通过context.ReturnValue来获取实际泛型类型，但如果返回值时null，那么将抛出NullReferenceException，
            // 预计将在MethodContext中新增一个属性记录泛型返回值类型，在初始化MethodContext时传入，但此种方式将会产生较大改动，在考虑到Task<>/ValueTask<>返回
            // null值场景几乎没有，暂时使用该方法临时修复泛型产生的bug
            if (TryResolve(context.RealReturnType!, context.ReturnValue!, out var func))
            {
                if (!context.ExContinueOnce)
                {
                    var returnValue = func!(context.ReturnValue!, context);
                    context.ReplaceReturnValue(this, returnValue, false);
                    context.ExContinueOnce = true;
                }
            }
            else
            {
                ExOnSuccess(context);
                if (context.ExReturnValueReplaced)
                {
                    context.ReplaceReturnValue(this, context.ExReturnValue!, false);
                }
            }
            context.ExMode = false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public sealed override void OnExit(MethodContext context)
        {
            context.ExMode = true;
            if (context.RealReturnType == context.ExReturnType || context.Datas.Contains(EX_SYNC_EXCEPTION))
            {
                ExOnExit(context);
            }
            context.ExMode = false;
        }

        private void SetReturnValue(MethodContext context)
        {
            if (!context.IsAsync)
            {
                if (typeof(Task) == context.RealReturnType)
                {
                    context.ReplaceReturnValue(this, Task.CompletedTask, false);
                }
                else if (typeof(Task).IsAssignableFrom(context.RealReturnType))
                {
                    context.ReplaceReturnValue(this, context.RealReturnType!.NewTaskResult(context.ExReturnValue!), false);
                }
                else if (typeof(ValueTask) == context.RealReturnType)
                {
                    context.ReplaceReturnValue(this, default(ValueTask), false);
                }
                else if (context.RealReturnType!.FullName.StartsWith(Constants.FULLNAME_ValueTask))
                {
                    context.ReplaceReturnValue(this, context.RealReturnType!.NewValueTaskResult(context.ExReturnValue!), false);
                }
                else
                {
                    context.ReplaceReturnValue(this, context.ExReturnValue!, false);
                }
            }
            else
            {
                context.ReplaceReturnValue(this, context.ExReturnValue!, false);
            }
        }

        #region Resolve
        private static bool TryResolve(Type type, object returnValue, out Func<object, MethodContext, object>? func)
        {
            // todo: 对于泛型Task<>/ValueTask<>返回值，目前通过context.ReturnValue来获取实际泛型类型，但如果返回值时null，那么将抛出NullReferenceException，
            // 预计将在MethodContext中新增一个属性记录泛型返回值类型，在初始化MethodContext时传入，但此种方式将会产生较大改动，在考虑到Task<>/ValueTask<>返回
            // null值场景几乎没有，暂时使用该方法临时修复泛型产生的bug
            if (typeof(Task) == type || typeof(ValueTask) == type)
            {
                _cache.TryGetValue(type, out func);
                return true;
            }
            if (typeof(Task).IsAssignableFrom(type))
            {
                func = _cache.GetOrAdd(returnValue.GetType(), ResolveGenericTask);
                return true;
            }
            if (!type.IsGenericParameter && type.FullName.StartsWith(Constants.FULLNAME_ValueTask))
            {
                func = _cache.GetOrAdd(returnValue.GetType(), ResolveGenericValueTask);
                return true;
            }

            func = null;
            return false;
        }

        private static Func<object, MethodContext, object> ResolveGenericTask(Type type)
        {
            var methodInfo = typeof(ExMoAttribute).GetMethod(nameof(GenericTaskContinueWith), BindingFlags.NonPublic | BindingFlags.Static);
            var genericMethodInfo = methodInfo.MakeGenericMethod(type.GetGenericArguments()[0]);
            var @delegate = genericMethodInfo.CreateDelegate(typeof(Func<object, MethodContext, object>));
            return (Func<object, MethodContext, object>)@delegate;
        }

        private static Func<object, MethodContext, object> ResolveGenericValueTask(Type type)
        {
            var methodInfo = typeof(ExMoAttribute).GetMethod(nameof(GenericValueTaskContinueWith), BindingFlags.NonPublic | BindingFlags.Static);
            var genericMethodInfo = methodInfo.MakeGenericMethod(type.GetGenericArguments()[0]);
            var @delegate = genericMethodInfo.CreateDelegate(typeof(Func<object, MethodContext, object>));
            return (Func<object, MethodContext, object>)@delegate;
        }
        #endregion Resolve

        #region ContinueWith
        private static object TaskContinueWith(object taskObject, MethodContext context)
        {
            var tcs = new TaskCompletionSource<bool>();
            ((Task)taskObject).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var exception = t.Exception.InnerExceptions.Count == 1 ? t.Exception.InnerException : t.Exception;
                    context.Exception = exception;
                    context.Mos.ForEach(!context.MosNonEntryFIFO, mo =>
                    {
                        if (mo is ExMoAttribute exmo)
                        {
                            context.ExMode = true;
                            exmo.ExOnException(context);
                            context.ExMode = false;
                        }
                    });
                    if (!context.ExceptionHandled)
                    {
                        context.Mos.ForEach(!context.MosNonEntryFIFO, mo =>
                        {
                            if (mo is ExMoAttribute exmo)
                            {
                                context.ExMode = true;
                                exmo.ExOnExit(context);
                                context.ExMode = false;
                            }
                        });
                        tcs.TrySetException(exception);
                        return;
                    }
                }
                else
                {
                    context.Mos.ForEach(!context.MosNonEntryFIFO, mo =>
                    {
                        if (mo is ExMoAttribute exmo)
                        {
                            context.ExMode = true;
                            exmo.ExOnSuccess(context);
                            context.ExMode = false;
                        }
                    });
                }
                context.Mos.ForEach(!context.MosNonEntryFIFO, mo =>
                {
                    if (mo is ExMoAttribute exmo)
                    {
                        context.ExMode = true;
                        exmo.ExOnExit(context);
                        context.ExMode = false;
                    }
                });
                tcs.TrySetResult(true);
            });

            return tcs.Task;
        }

        private static object ValueTaskContinueWith(object valueTaskObject, MethodContext context)
        {
            var task = ((ValueTask)valueTaskObject).AsTask();
            task = (Task)TaskContinueWith(task, context);
            return new ValueTask(task);
        }

        private static object GenericTaskContinueWith<T>(object taskObject, MethodContext context)
        {
            var tcs = new TaskCompletionSource<T>();
            ((Task<T>)taskObject).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var exception = t.Exception.InnerExceptions.Count == 1 ? t.Exception.InnerException : t.Exception;
                    context.Exception = exception;
                    context.Mos.ForEach(!context.MosNonEntryFIFO, mo =>
                    {
                        if (mo is ExMoAttribute exmo)
                        {
                            context.ExMode = true;
                            exmo.ExOnException(context);
                            context.ExMode = false;
                        }
                    });
                    context.Mos.ForEach(!context.MosNonEntryFIFO, mo =>
                    {
                        if (mo is ExMoAttribute exmo)
                        {
                            context.ExMode = true;
                            exmo.ExOnExit(context);
                            context.ExMode = false;
                        }
                    });
                    if (context.ExceptionHandled)
                    {
                        tcs.TrySetResult((T)context.ExReturnValue!);
                    }
                    else
                    {
                        tcs.TrySetException(exception);
                    }
                }
                else
                {
                    context.Mos.ForEach(!context.MosNonEntryFIFO, mo =>
                    {
                        if (mo is ExMoAttribute exmo)
                        {
                            context.ExMode = true;
                            exmo.ExOnSuccess(context);
                            context.ExMode = false;
                        }
                    });
                    context.Mos.ForEach(!context.MosNonEntryFIFO, mo =>
                    {
                        if (mo is ExMoAttribute exmo)
                        {
                            context.ExMode = true;
                            exmo.ExOnExit(context);
                            context.ExMode = false;
                        }
                    });
                    if (context.ExReturnValueReplaced)
                    {
                        tcs.TrySetResult((T)context.ExReturnValue!);
                    }
                    else
                    {
                        tcs.TrySetResult(t.Result);
                    }
                }
            });

            return tcs.Task;
        }

        private static object GenericValueTaskContinueWith<T>(object valueTaskObject, MethodContext context)
        {
            var task = ((ValueTask<T>)valueTaskObject).AsTask();
            task = (Task<T>)GenericTaskContinueWith<T>(task, context);
            return new ValueTask<T>(task);
        }
        #endregion ContinueWith
    }
}
