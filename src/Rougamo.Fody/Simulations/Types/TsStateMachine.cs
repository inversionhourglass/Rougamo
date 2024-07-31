using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal abstract class TsStateMachine(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        public FieldSimulation<TsArray<TsMo>>? F_MoArray { get; protected set; }

        public FieldSimulation<TsMo>[]? F_Mos { get; protected set; }

        public FieldSimulation<TsMethodContext> F_MethodContext { get; protected set; }

        public FieldSimulation F_State { get; protected set; }

        public FieldSimulation[] F_Parameters { get; protected set; }

        public FieldSimulation<TsWeavingTarget>? F_DeclaringThis { get; protected set; }

        public MethodSimulation M_MoveNext => MethodSimulate(Constants.METHOD_MoveNext, false);
    }
}
