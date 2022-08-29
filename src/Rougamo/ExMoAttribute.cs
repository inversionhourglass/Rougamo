using Rougamo.Context;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Rougamo
{
    public class ExMoAttribute : MoAttribute
    {
        private static ConcurrentDictionary<Type, Func<ExMoAttribute, MethodContext, object>> _cache = new();

        private object OnSuccess_(ExMoAttribute exmo, MethodContext context)
        {
            var task = (Task<int>)context.ReturnValue!;
            var tcs = new TaskCompletionSource<int>();
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var exception = t.Exception.InnerExceptions.Count == 1 ? t.Exception.InnerException : t.Exception;
                    exmo._OnException(context);
                    if (context.ExceptionHandled)
                    {
                        tcs.TrySetResult((int)context.ReturnValue!);
                    }
                    else
                    {
                        tcs.TrySetException(exception);
                    }
                }
                else
                {
                    var result = t.Result;
                    exmo._OnSuccess(context);
                    if (context.ReturnValueReplaced)
                    {
                        result = (int)context.ReturnValue!;
                    }
                    tcs.TrySetResult(result);
                }
            });
            return tcs.Task;
        }

        public sealed override void OnException(MethodContext context)
        {
            context.VerifyReplacement = false;
            _OnException(context);
            if (context.ExceptionHandled)
            {
                SetReturnValue(context);
            }
        }

        public sealed override void OnSuccess(MethodContext context)
        {
            if (typeof(Task) == context.RealReturnType)
            {
                var tcs = new TaskCompletionSource<bool>();
                ((Task)context.ReturnValue!).ContinueWith(t =>
                {
                    context.VerifyReplacement = false;
                    if (t.IsFaulted)
                    {
                        var exception = t.Exception.InnerExceptions.Count == 1 ? t.Exception.InnerException : t.Exception;
                        context.Exception = exception;
                        _OnException(context);
                        if (!context.ExceptionHandled)
                        {
                            tcs.TrySetException(exception);
                        }
                    }
                    else
                    {
                        _OnSuccess(context);
                    }
                });
                context.ReplaceReturnValue(this, tcs.Task);
            }
            else if (typeof(Task).IsAssignableFrom(context.RealReturnType))
            {
                //var returnType = context.RealReturnType.GetGenericArguments()[0];
                //var continueWith = _cache.GetOrAdd(returnType, type =>
                //{
                //    var tcsType = typeof(TaskCompletionSource<>).MakeGenericType(type);

                //    var method = new DynamicMethod($"{type.FullName}_ContinueWith", typeof(object), new[] { typeof(MethodContext), typeof(Type) });
                //    var generator = method.GetILGenerator();

                    

                //    return (Func<MethodContext, Type, object>)method.CreateDelegate(typeof(Func<MethodContext, Type, object>));
                //});
                //var @return = continueWith(context, returnType);
                //context.ReplaceReturnValue(this, @return);

                //var @return = ((Task)context.ReturnValue).ContinueWith(t =>
                //{
                //    if (t.Status == TaskStatus.Faulted)
                //    {
                //        context.Exception = t.Exception;
                //        _OnException(context);
                //    }
                //    else
                //    {
                //        var result = ((Task<dynamic>)t).Result;
                //        context.ReturnValue = result;
                //        _OnSuccess(context);
                //        return context.ReturnValueReplaced ? context.ReturnValue : result;
                //    }
                //});
            }
            else
            {
                _OnSuccess(context);
            }
        }

        private void SetReturnValue(MethodContext context)
        {
            if (!context.RealReturnType!.Setable(context.ReturnValue!))
            {
                if (typeof(Task) == context.RealReturnType && context.ReturnValue == null)
                {
                    context.ReturnValue = Task.CompletedTask;
                }
                else if (typeof(Task).IsAssignableFrom(context.RealReturnType))
                {
                    context.ReturnValue = context.RealReturnType!.NewTaskResult(context.ReturnValue!);
                }
                else if(typeof(ValueTask) == context.RealReturnType && context.ReturnValue == null)
                {
                    context.ReturnValue = default(ValueTask);
                }
                else if (context.RealReturnType!.FullName.StartsWith("System.Threading.Tasks.ValueTask"))
                {
                    context.ReturnValue = context.RealReturnType!.NewValueTaskResult(context.ReturnValue!);
                }
                else
                {
                    throw new ArgumentException($"Method return type({context.RealReturnType?.FullName}) is not assignable from returnvalue({context.ReturnValue?.GetType().FullName})");
                }
            }
        }

        protected virtual void _OnException(MethodContext context) { }

        protected virtual void _OnSuccess(MethodContext context) { }
    }
}
