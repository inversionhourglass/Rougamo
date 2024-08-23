using Mono.Cecil;

namespace Fody.Simulations.Types
{
    public class TsAsyncIteratorMethodBuilder(TypeReference typeRef, IHost? host, SimulationModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver), IAsyncBuilder
    {
        public MethodSimulation M_AwaitUnsafeOnCompleted => PublicMethodSimulate(Constants.METHOD_AwaitUnsafeOnCompleted, false);

        public MethodSimulation M_Complete => PublicMethodSimulate(Constants.METHOD_Complete, false);
    }
}
