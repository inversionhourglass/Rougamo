using Rougamo.Context;
using Rougamo;

namespace BasicUsage.Mos
{
    public struct ValueOmitArguments : IMo
    {
        public AccessFlags Flags => AccessFlags.All;

        public string Pattern => null;

        public Feature Features => Feature.EntryReplace;

        public double Order => 1;

        public Omit MethodContextOmits => Omit.Arguments;

        public void OnEntry(MethodContext context)
        {
            if (context.Arguments.Length == 0)
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
