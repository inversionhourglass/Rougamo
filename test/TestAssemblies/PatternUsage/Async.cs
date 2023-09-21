using PatternUsage.Attributes.Methods;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PatternUsage
{
    [GenericVoTaskReturn]
    [NoGenericVoTaskReturn]
    [DoubleOrIntVoTaskReturn]
    public class Async : NonPublicCaller
    {
        public static void Sync(List<string> executedMos) { }

        protected internal async void AsyncVoid(List<string> executedMos) => await Task.Yield();

        private Task Task1(List<string> executedMos) => Task.CompletedTask;

        protected static ValueTask ValueTask1(List<string> executedMos) => new ValueTask();

        internal async Task TaskAsync(List<string> executedMos) => await Task.Yield();

        private static protected async ValueTask ValueTaskAsync(List<string> executedMos) => await Task.Yield();

        public Task<int> TaskInt(List<string> executedMos) => Task.FromResult(123);

        Task<string> TaskString(List<string> executedMos) => Task.FromResult(string.Empty);

        ValueTask<int> ValueTaskInt(List<string> executedMos) => new(1);

        static ValueTask<string> ValueTaskString(List<string> executedMos) => new(string.Empty);

        private async Task<int> TaskIntAsync(List<string> executedMos)
        {
            await Task.Yield();
            return 1;
        }

        protected static async ValueTask<string> ValueTaskStringAsync(List<string> executedMos)
        {
            await Task.Yield();
            return string.Empty;
        }

        static async Task<double> TaskDoubleAsync(List<string> executedMos)
        {
            await Task.Yield();
            return double.MaxValue;
        }
    }
}
