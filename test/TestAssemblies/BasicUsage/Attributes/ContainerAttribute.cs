using Rougamo;
using Rougamo.Context;
using System;

namespace BasicUsage.Attributes
{
    public abstract class ContainerAttribute : MoAttribute
    {
        protected void SetContainer(MethodContext context)
        {
            if (context.Target == null) return;

            if (context.Target is not IMoDataContainer container) throw new ArgumentException("test class should be inherit from IMoDataContainer");
            container.Mo = this;
            container.Context = context;
        }

        public override void OnEntry(MethodContext context)
        {
            SetContainer(context);
        }

        public override void OnException(MethodContext context)
        {
            SetContainer(context);
        }

        public override void OnSuccess(MethodContext context)
        {
            SetContainer(context);
        }

        public override void OnExit(MethodContext context)
        {
            SetContainer(context);
        }
    }
}
