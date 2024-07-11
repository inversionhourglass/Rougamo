using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsStateMachineDeclaringThis(TypeReference typeRef, IHost? host, ModuleDefinition moduleDef) : TypeSimulation(typeRef, host, moduleDef)
    {
    }
}
