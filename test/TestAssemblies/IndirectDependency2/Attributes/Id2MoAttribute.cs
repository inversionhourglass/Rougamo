using Rougamo;
using Rougamo.Context;
using System;

namespace IndirectDependency2.Attributes
{
    public class Id2MoAttribute : MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            this.SetOnEntry(context);
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Id2MoAttribute<T> : MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            this.SetOnEntry(context);
        }
    }
}
