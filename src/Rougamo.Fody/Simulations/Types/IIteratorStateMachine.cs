using Fody.Simulations;
using Fody.Simulations.Types;

namespace Rougamo.Fody.Simulations.Types
{
    internal interface IIteratorStateMachine : IStateMachine
    {
        FieldSimulation F_Current { get; }

        FieldSimulation<TsList>? F_Items { get; }
    }
}
