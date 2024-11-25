using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;

namespace BasicUsage.Mos
{
    [Optimization(MethodContext = Omit.Mos)]
    [Advice(Feature.OnEntry)]
    public class ClassOmitMos : MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            if (context.Mos.Count == 0)
            {
                this.SetOnEntry(context);
            }
        }
    }
}
