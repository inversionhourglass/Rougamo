using Fody.Simulations;
using Fody.Simulations.Types;

namespace Rougamo.Fody.Simulations.Types
{
    internal interface IStateMachine : IParameterSimulation
    {
        FieldSimulation<TsArray<TsMo>>? F_MoArray { get; }

        FieldSimulation<TsMo>[]? F_Mos { get; }

        FieldSimulation<TsMethodContext> F_MethodContext { get; }

        FieldSimulation F_State { get; }

        FieldSimulation[] F_Parameters { get; }

        FieldSimulation<TsWeavingTarget>? F_DeclaringThis { get; }

        MethodSimulation M_MoveNext { get; }
    }
}
