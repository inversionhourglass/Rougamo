using Discoverers;
using Rougamo;
using Rougamo.Context;
using System;

namespace BasicUsage.Attributes
{
    public class PublicDiscoveryAttribute : MoAttribute
    {
        public override Type DiscovererType => typeof(PublicDiscoverer);

        public override Feature Features => Feature.OnEntry;

        public override void OnEntry(MethodContext context)
        {
            ((IRecording)context.Target).Recording.Add(Order.ToString());
        }
    }
}
