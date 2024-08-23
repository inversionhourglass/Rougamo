namespace Fody.Simulations.Types
{
    public interface IAsyncBuilder
    {
        MethodSimulation M_AwaitUnsafeOnCompleted { get; }
    }
}
