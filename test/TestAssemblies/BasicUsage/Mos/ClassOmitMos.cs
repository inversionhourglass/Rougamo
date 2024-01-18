using Rougamo;
using Rougamo.Context;

namespace BasicUsage.Mos
{
    public class ClassOmitMos : MoAttribute
    {
        public override Feature Features => Feature.OnEntry;

        public override Omit MethodContextOmits => Omit.Mos;

        public override void OnEntry(MethodContext context)
        {
            if (context.Mos.Count == 0)
            {
                this.SetOnEntry(context);
            }
        }
    }
}
