using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal class TsWeavingTarget(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        public MethodSimulation M_Actual { get; set; }

        public MethodSimulation M_Proxy { get; set; }

        public TsWeavingTarget SetMethods(MethodDefinition actualMethodDef, MethodDefinition proxyMethodDef)
        {
            M_Actual = actualMethodDef.Simulate(this);
            M_Proxy = proxyMethodDef.Simulate(this);

            return this;
        }
    }
}
