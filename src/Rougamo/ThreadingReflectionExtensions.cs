using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rougamo
{
    internal static class ThreadingReflectionExtensions
    {
        private static readonly ConcurrentDictionary<Type, Func<object, object>> _taskCache = new();
        private static readonly ConcurrentDictionary<Type, Func<object, object>> _valueTaskCache = new();

        public static object NewTaskResult(this Type taskType, object returnValue)
        {
            var func = _taskCache.GetOrAdd(taskType, type =>
            {
                var returnType = type.GetGenericArguments().Single();
                var methodInfo = typeof(ThreadingReflectionExtensions).GetMethod(nameof(NewTaskResult), BindingFlags.NonPublic | BindingFlags.Static);
                var genericMethodInfo = methodInfo.MakeGenericMethod(returnType);
                var @delegate = genericMethodInfo.CreateDelegate(typeof(Func<object, object>));
                return (Func<object, object>)@delegate;
            });

            return func(returnValue);
        }

        public static object NewValueTaskResult(this Type valueTaskType, object returnValue)
        {
            var func = _valueTaskCache.GetOrAdd(valueTaskType, type =>
            {
                var returnType = type.GetGenericArguments().Single();
                var methodInfo = typeof(ThreadingReflectionExtensions).GetMethod(nameof(NewValueTaskResult), BindingFlags.NonPublic | BindingFlags.Static);
                var genericMethodInfo = methodInfo.MakeGenericMethod(returnType);
                var @delegate = genericMethodInfo.CreateDelegate(typeof(Func<object, object>));
                return (Func<object, object>)@delegate;
            });

            return func(returnValue);
        }

        private static object NewTaskResult<T>(object value) => Task.FromResult((T)value);

        private static object NewValueTaskResult<T>(object value) => new ValueTask<T>((T)value);
    }
}
