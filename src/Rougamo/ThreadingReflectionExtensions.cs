using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rougamo
{
    internal static class ThreadingReflectionExtensions
    {
        private static ConcurrentDictionary<Type, Func<object, object>> _taskCache = new();
        private static ConcurrentDictionary<Type, Func<object, object>> _valueTaskCache = new();

        public static object NewTaskResult(this Type taskType, object returnValue)
        {
            var func = _taskCache.GetOrAdd(taskType, type =>
            {
                var returnType = type.GetGenericArguments().Single();
                var methodFromResult = typeof(Task).GetMethod("FromResult").MakeGenericMethod(returnType);
                var parameterExpression = Expression.Parameter(typeof(object), "obj");
                var convertExpression = Expression.Convert(parameterExpression, returnType);
                var callExpression = Expression.Call(methodFromResult, convertExpression);

                var targetFuncType = typeof(Func<,>).MakeGenericType(typeof(object), typeof(object));
                var targetExpressionType = typeof(Expression<>).MakeGenericType(targetFuncType);
                var lambdaMethod = typeof(Expression).GetMethod("Lambda", 1, typeof(Expression), typeof(ParameterExpression[]));
                var genericLambdaMethod = lambdaMethod.MakeGenericMethod(targetFuncType);
                var lambda = genericLambdaMethod.Invoke(null, new object[] { callExpression, new ParameterExpression[] { parameterExpression } });

                var compileMethod = targetExpressionType.GetMethod("Compile", new Type[0]);
                var res = compileMethod.Invoke(lambda, null);
                return (Func<object, object>)res;
            });

            return func(returnValue);
        }

        public static object NewValueTaskResult(this Type valueTaskType, object returnValue)
        {
            var func = _valueTaskCache.GetOrAdd(valueTaskType, type =>
            {
                var returnType = type.GetGenericArguments().Single();
                var valueTaskCtor = type.GetConstructor(new[] { returnType });
                var parameterExpression = Expression.Parameter(typeof(object), "obj");
                var aimConvertExpression = Expression.Convert(parameterExpression, returnType);
                var newExpression = Expression.New(valueTaskCtor, aimConvertExpression);
                var objConvertExpression = Expression.Convert(newExpression, typeof(object));

                var targetFuncType = typeof(Func<,>).MakeGenericType(typeof(object), typeof(object));
                var targetExpressionType = typeof(Expression<>).MakeGenericType(targetFuncType);
                var lambdaMethod = typeof(Expression).GetMethod("Lambda", 1, typeof(Expression), typeof(ParameterExpression[]));
                var genericLambdaMethod = lambdaMethod.MakeGenericMethod(targetFuncType);
                var lambda = genericLambdaMethod.Invoke(null, new object[] { objConvertExpression, new ParameterExpression[] { parameterExpression } });

                var compileMethod = targetExpressionType.GetMethod("Compile", new Type[0]);
                var res = compileMethod.Invoke(lambda, null);
                return (Func<object, object>)res;
            });

            return func(returnValue);
        }
    }
}
