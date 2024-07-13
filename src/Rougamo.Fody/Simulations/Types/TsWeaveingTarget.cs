using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsWeaveingTarget(TypeReference typeRef, IHost? host, ModuleDefinition moduleDef) : TypeSimulation(typeRef, host, moduleDef)
    {
    }
}
