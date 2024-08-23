using Mono.Cecil;

namespace Fody.Simulations.Types
{
    public class TsList(TypeReference typeRef, IHost? host, SimulationModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
    {
        public MethodSimulation M_Add => MethodSimulate(Constants.METHOD_Add, true);

        public MethodSimulation M_ToArray => MethodSimulate(Constants.METHOD_ToArray, true);
    }
}
