namespace Rougamo.Fody.Simulations.Types
{
    internal interface IAsyncStateMachine : IStateMachine
    {
        FieldSimulation<TsAwaiter> F_Awaiter { get; }

        FieldSimulation<TsAwaiter> F_MoAwaiter { get; }

        IAsyncBuilder FV_Builder { get; }
    }
}
