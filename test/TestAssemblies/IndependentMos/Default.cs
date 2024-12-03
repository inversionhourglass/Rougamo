using Rougamo;
using Rougamo.Context;

namespace IndependentMos
{
    public class Default : Mo
    {
        public override void OnEntry(MethodContext context)
        {
            this.SetOnEntry(context);
        }
    }
}
