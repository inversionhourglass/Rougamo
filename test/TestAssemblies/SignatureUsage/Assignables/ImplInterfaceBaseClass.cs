using System.Threading.Tasks;

namespace SignatureUsage.Assignables
{
    public class ImplInterfaceBaseClass : BaseClass, Interface
    {
        public (string, int) ImplInterfaceBase() => default;

        public override Interface GetInterface() => default;

        public AbstractClass GetAbstract() => default;

        public Task CompletedAsync() => Task.CompletedTask;

        private async Task YieldAsync() => await Task.Yield();

        public ValueTask DefaultAsync() => new ValueTask();
    }
}
