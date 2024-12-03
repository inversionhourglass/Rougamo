using Rougamo;
using Rougamo.Context;

namespace IndependentMos.Attributes
{
    public abstract class SetOnEntryMoAttribute : MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            this.SetOnEntry(context);
        }
    }
}
