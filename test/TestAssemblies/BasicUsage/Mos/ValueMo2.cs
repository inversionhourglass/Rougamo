using Rougamo;
using Rougamo.Context;

namespace BasicUsage.Mos
{
    public struct ValueMo2 : IMo
    {
        public AccessFlags Flags => AccessFlags.All;

        public string Pattern => null;

        public Feature Features => Feature.OnEntry;

        public double Order => 1;

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
