using System.Threading.Tasks;

namespace SignatureUsage.Assignables
{
    public class ImplBaseClass : BaseClass
    {
        public decimal ImplBase() => default;

        public AbstractClass GetAbstract() => default;

        public BaseClass GetBase() => default;

        public override Interface GetInterface() => default;

        public Task<string> GetStringAsync() => Task.FromResult(string.Empty);

        public async ValueTask<int> GetIntAsync()
        {
            await Task.Yield();
            return 0;
        }
    }
}
