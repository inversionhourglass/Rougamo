using Rougamo.Context;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private const string EX_MOS_DATA_KEY = "rougamo.ex.mos";
        private const string EX_SYNC_EXCEPTION = "rougamo.ex.syncException";
        private const string REVERSE_CALL_KEY = "rougamo.reverse";
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
            AttachSelf(context);
            context.ExMode = true;
            ExOnEntry(context);
            if (context.ExReturnValueReplaced)
            {
                SetReturnValue(context);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public sealed override void OnException(MethodContext context)
        {
            SetReverse(context);
            context.Datas[EX_SYNC_EXCEPTION] = true;
            ExOnException(context);
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
            SetReverse(context);
            if (TryResolve(context.RealReturnType!, out var func))
            {
                if (context.Datas.Contains(EX_MOS_DATA_KEY))
                {
                    var returnValue = func!(context.ReturnValue!, context);
                    context.ReplaceReturnValue(this, returnValue, false);
                    context.Datas.Remove(EX_MOS_DATA_KEY);
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
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public sealed override void OnExit(MethodContext context)
        {
            if(context.RealReturnType == context.ExReturnType || context.Datas.Contains(EX_SYNC_EXCEPTION))
            {
                ExOnExit(context);
            }
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

        private void AttachSelf(MethodContext context)
        {
            if (!context.Datas.Contains(EX_MOS_DATA_KEY))
            {
                context.Datas.Add(EX_MOS_DATA_KEY, new List<ExMoAttribute>());
            }
            var mos = (List<ExMoAttribute>)context.Datas[EX_MOS_DATA_KEY];
            mos.Add(this);
        }

        private void SetReverse(MethodContext context)
        {
            if (!context.Datas.Contains(REVERSE_CALL_KEY))
            {
                context.Datas[REVERSE_CALL_KEY] = ((List<ExMoAttribute>)context.Datas[EX_MOS_DATA_KEY])[0] != this;
            }
        }

        #region Resolve
        private static bool TryResolve(Type type, out Func<object, MethodContext, object>? func)
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
            var mos = (List<ExMoAttribute>)context.Datas[EX_MOS_DATA_KEY];
            var reverse = (bool)context.Datas[REVERSE_CALL_KEY];
            var tcs = new TaskCompletionSource<bool>();
            ((Task)taskObject).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var exception = t.Exception.InnerExceptions.Count == 1 ? t.Exception.InnerException : t.Exception;
                    context.Exception = exception;
                    mos.ForEach(reverse, mo =>
                    {
                        mo.ExOnException(context);
                    });
                    if (!context.ExceptionHandled)
                    {
                        mos.ForEach(reverse, mo =>
                        {
                            mo.ExOnExit(context);
                        });
                        tcs.TrySetException(exception);
                        return;
                    }
                }
                else
                {
                    mos.ForEach(reverse, mo =>
                    {
                        mo.ExOnSuccess(context);
                    });
                }
                mos.ForEach(reverse, mo =>
                {
                    mo.ExOnExit(context);
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
            var mos = (List<ExMoAttribute>)context.Datas[EX_MOS_DATA_KEY];
            var reverse = (bool)context.Datas[REVERSE_CALL_KEY];
            var tcs = new TaskCompletionSource<T>();
            ((Task<T>)taskObject).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var exception = t.Exception.InnerExceptions.Count == 1 ? t.Exception.InnerException : t.Exception;
                    context.Exception = exception;
                    mos.ForEach(reverse, mo =>
                    {
                        mo.ExOnException(context);
                    });
                    mos.ForEach(reverse, mo =>
                    {
                        mo.ExOnExit(context);
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
                    mos.ForEach(reverse, mo =>
                    {
                        mo.ExOnSuccess(context);
                    });
                    mos.ForEach(reverse, mo =>
                    {
                        mo.ExOnExit(context);
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
