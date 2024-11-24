using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;

namespace BasicUsage.Mos
{
    [Advice(Feature.OnEntry)]
    public class ClassOmitMos : MoAttribute
    {
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
