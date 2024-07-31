using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsList(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
    {
        public MethodSimulation M_Add => MethodSimulate(Constants.METHOD_Add, true);

        public MethodSimulation M_ToArray => MethodSimulate(Constants.METHOD_ToArray, true);
    }
}
