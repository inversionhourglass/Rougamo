using Rougamo;
using Rougamo.Context;
using System;
using System.Collections.Generic;
using System.Data;

namespace BasicUsage.Attributes
{
    public class RewriteArgsAttribute : MoAttribute
    {
        private static readonly IReadOnlyDictionary<Type, object> _Values = new Dictionary<Type, object>
        {
            { typeof(string), $"{nameof(RewriteArgsAttribute)}-Replaced" },
            { typeof(sbyte), (sbyte)127 },
            { typeof(int?), 999 },
            { typeof(double), Math.PI },
            { typeof(object), new object() },
            { typeof(Type), typeof(RewriteArgsAttribute) },
            { typeof(DbType[]), new []{ DbType.Int32, DbType.Single} },
            { typeof(DbType), DbType.Binary },
            { typeof(DateTime), DateTime.MaxValue }
        };

        public static IReadOnlyDictionary<Type, object> Values => _Values;

        public override void OnEntry(MethodContext context)
        {
            context.RewriteArguments = true;

            var parameters = context.Method.GetParameters();
            for (var i = 0; i < context.Arguments.Length; i++)
            {
                var argType = parameters[i].ParameterType;
                if (argType.IsByRef)
                {
                    argType = argType.GetElementType();
                }
                if (argType.IsGenericParameter)
                {
                    if (context.Arguments[i] == null) continue;
                    argType = context.Arguments[i].GetType();
                }
                if (Values.ContainsKey(argType))
                {
                    context.Arguments[i] = Values[argType];
                }
            }
        }
    }
}
