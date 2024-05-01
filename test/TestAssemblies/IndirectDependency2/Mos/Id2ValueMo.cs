using Rougamo;
using Rougamo.Context;

namespace IndirectDependency2.Mos
{
    public struct Id2ValueMo : IMo
    {
        public AccessFlags Flags => AccessFlags.InstancePublic;

        public string Pattern => null;

        public Feature Features => Feature.All;

        public double Order => 0;

        public Omit MethodContextOmits => Omit.None;

        public void OnEntry(MethodContext context)
        {
            this.SetOnEntry(context);
        }

        public void OnException(MethodContext context)
        {
        }

        public void OnExit(MethodContext context)
        {
        }

        public void OnSuccess(MethodContext context)
        {
        }
    }

    public struct Id2ValueMo<T> : IMo
    {
        public AccessFlags Flags => AccessFlags.InstancePublic;

        public string Pattern => null;

        public Feature Features => Feature.All;

        public double Order => 0;

        public Omit MethodContextOmits => Omit.None;

        public void OnEntry(MethodContext context)
        {
            this.SetOnEntry(context);
        }

        public void OnException(MethodContext context)
        {
        }

        public void OnExit(MethodContext context)
        {
        }

        public void OnSuccess(MethodContext context)
        {
        }
    }
}
