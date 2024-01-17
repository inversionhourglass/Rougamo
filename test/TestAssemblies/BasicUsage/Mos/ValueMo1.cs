using Rougamo;
using Rougamo.Context;

namespace BasicUsage.Mos
{
    public struct ValueMo1 : IMo
    {
        public AccessFlags Flags => AccessFlags.All;

        public string Pattern => "execution(async null *(..))";

        public Feature Features => Feature.OnEntry;

        public double Order => 1;

        public bool OmitMos => false;

        public bool OmitArguments => false;

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
