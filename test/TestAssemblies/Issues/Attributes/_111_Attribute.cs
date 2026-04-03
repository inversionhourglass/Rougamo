using Rougamo;
using Rougamo.Context;
using System.Collections.Generic;

namespace Issues.Attributes;

public class _111_Attribute : MoAttribute
{
    public override void OnEntry(MethodContext context)
    {
        ((List<string>)context.Arguments[0]).Add(nameof(OnEntry));
    }

    public override void OnSuccess(MethodContext context)
    {
        ((List<string>)context.Arguments[0]).Add(nameof(OnSuccess));
    }

    public override void OnException(MethodContext context)
    {
        ((List<string>)context.Arguments[0]).Add(nameof(OnException));
    }

    public override void OnExit(MethodContext context)
    {
        ((List<string>)context.Arguments[0]).Add(nameof(OnExit));
    }
}
