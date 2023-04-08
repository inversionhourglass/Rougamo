using Discoverers;
using Rougamo;
using Rougamo.Context;

namespace BasicUsage.Attributes
{
    public class PublicDiscoveryAttribute : MoAttribute
    {
        public PublicDiscoveryAttribute()
        {
            DiscovererType = typeof(PublicDiscoverer);
        }

        public override Feature Features => Feature.OnEntry;

        public override void OnEntry(MethodContext context)
        {
            ((IRecording)context.Target).Recording.Add(Order.ToString());
        }
    }
}
