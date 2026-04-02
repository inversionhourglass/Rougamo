using Rougamo;
using Rougamo.Context;
using System.Collections.Generic;

namespace Issues.Attributes;

public class _110_Attribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        ((List<string>)context.Arguments[0]).Add($"{GetMethodName(context)} OnEntry");
    }

    public override void OnSuccess(MethodContext context)
    {
        ((List<string>)context.Arguments[0]).Add($"{GetMethodName(context)} OnSuccess");
    }

    public override void OnException(MethodContext context)
    {
        ((List<string>)context.Arguments[0]).Add($"{GetMethodName(context)} OnException");
    }

    public override void OnExit(MethodContext context)
    {
        ((List<string>)context.Arguments[0]).Add($"{GetMethodName(context)} OnExit");
    }

    public static string GetMethodName(MethodContext context)
    {
        if (!context.Method.IsGenericMethodDefinition) return context.Method.Name;

        var count = context.Method.GetGenericArguments().Length;

        return $"{context.Method.Name}`{count}";
    }
}
