using Rougamo;
using Rougamo.Context;
using Rougamo.Flexibility;
using Rougamo.Metadatas;
using System.Collections.Generic;

namespace Issues.Attributes
{
    public abstract class _97_Attribute : MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            if (context.Arguments.Length == 0 || context.Arguments[0] is not List<double> orders) return;

            orders.Add(((IFlexibleOrderable)this).Order);
        }
    }

    [Lifetime(Lifetime.Pooled)]
    [Advice(Feature.OnEntry)]
    public class _97_O1Attribute : _97_Attribute, IFlexibleOrderable
    {
        double IFlexibleOrderable.Order { get; set; } = 1;
    }

    [Lifetime(Lifetime.Pooled)]
    [Advice(Feature.OnEntry)]
    public class _97_O2Attribute : _97_Attribute, IFlexibleOrderable
    {
        double IFlexibleOrderable.Order { get; set; } = 2;
    }
}
