using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;
using System.Collections.Generic;

namespace BasicUsage.Attributes
{
    [Advice(Feature.OnEntry)]
    public abstract class TryLifetimeAttribute : MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            var list = (List<object>)context.Arguments[0];
            list.Add(this);
        }
    }

    [Lifetime(Lifetime.Singleton)]
    public class SingletonAttribute : TryLifetimeAttribute
    {
    }

    [Lifetime(Lifetime.Pooled)]
    public class PooledAttribute : TryLifetimeAttribute
    {
    }

    [Lifetime(Lifetime.Pooled)]
    public class PooledAttribute<T> : TryLifetimeAttribute, IResettable
    {
        public T Value { get; set; }

        public bool TryReset()
        {
            Value = default;
            return true;
        }
    }

    [Lifetime(Lifetime.Transient)]
    public class TransientAttribute : TryLifetimeAttribute
    {
    }
}
