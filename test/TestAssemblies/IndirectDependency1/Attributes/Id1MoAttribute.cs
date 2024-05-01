using Rougamo;
using Rougamo.Context;
using System;

namespace IndirectDependency1.Attributes
{
    public class Id1MoAttribute : MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            this.SetOnEntry(context);
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Id1MoAttribute<T> : MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            this.SetOnEntry(context);
        }
    }
}
