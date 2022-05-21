using Rougamo;
using Rougamo.Context;

namespace BasicUsage
{
    public interface IMoDataContainer
    {
        IMo Mo { get; set; }

        MethodContext Context { get; set; }
    }

    public class MoDataContainer : IMoDataContainer
    {
        public IMo Mo { get; set; }

        public MethodContext Context { get; set; }
    }
}
