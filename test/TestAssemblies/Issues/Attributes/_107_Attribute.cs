using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;

namespace Issues.Attributes;

[Advice(Feature.ExceptionHandle)]
internal class _107_Attribute(string message, object? defaultValue) : MoAttribute
{
    public override void OnException(MethodContext context)
    {
        context.HandledException(this, defaultValue!);
    }
}
