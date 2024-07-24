using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsWeaveingTarget(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
    {
    }
}
