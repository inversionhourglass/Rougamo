using Fody;
using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsWeaveingTarget(TypeReference typeRef, IHost? host, BaseModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
    {
    }
}
