using Fody.Simulations;
using Mono.Cecil;

namespace Fody
{
    public abstract class SimulationModuleWeaver : BaseModuleWeaver
    {
        protected internal TypeReference _tObjectRef;
        protected internal TypeReference _tBooleanRef;
        protected internal TypeReference _tInt32Ref;
        protected internal TypeReference _tTypeRef;
        protected internal TypeReference _tMethodBaseRef;

        protected internal MethodReference _mGetTypeFromHandleRef;
        protected internal MethodReference _mGetMethodFromHandleRef;

        protected internal GlobalSimulations _simulations;
    }
}
