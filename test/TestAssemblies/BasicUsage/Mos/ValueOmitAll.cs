using Rougamo.Context;
using Rougamo;

namespace BasicUsage.Mos
{
    public struct ValueOmitAll : IMo
    {
        public AccessFlags Flags => AccessFlags.All;

        public string Pattern => null;

        public Feature Features => Feature.EntryReplace;

        public double Order => 1;

        public Omit MethodContextOmits => Omit.All;

        public void OnEntry(MethodContext context)
        {
            if (context.Arguments.Length == 0 && context.Mos.Count == 0)
            {
                this.SetOnEntry(context);
            }
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
