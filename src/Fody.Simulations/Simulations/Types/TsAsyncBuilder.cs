using Mono.Cecil;

namespace Fody.Simulations.Types
{
    public class TsAsyncBuilder(TypeReference typeRef, IHost? host, SimulationModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver), IAsyncBuilder
    {
        public PropertySimulation P_Task => PropertySimulate(Constants.PROP_Task, true);

        public MethodSimulation M_AwaitUnsafeOnCompleted => PublicMethodSimulate(Constants.METHOD_AwaitUnsafeOnCompleted, true);

        public MethodSimulation M_SetException => PublicMethodSimulate(Constants.METHOD_SetException, true);

        public MethodSimulation M_SetResult => PublicMethodSimulate(Constants.METHOD_SetResult, true);

        public MethodSimulation M_Start => PublicMethodSimulate(Constants.METHOD_Start, true);

        public MethodSimulation M_SetStateMachine => PublicMethodSimulate(Constants.METHOD_SetStateMachine, true);
    }
}
