using BasicUsage.Mos;
using Rougamo;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BasicUsage
{
    [Rougamo<ValueOmitAll>]
    public class OmitUseCase
    {
        [Rougamo(typeof(ValueOmitMos))]
        public void Mos(List<string> executedMos)
        {

        }

        [Rougamo<ValueOmitArguments>]
        public List<string> Arguments(int x) => null;

        [Rougamo(typeof(ValueOmitArgumentsButFeature))]
        public List<string> ArgumentsFailed(int x) => null;

        [Rougamo(typeof(ValueOmitAll))]
        public static List<string> All(int x) => null;

        [Rougamo<ValueOmitMos>]
        public static async Task MosAsync(List<string> executedMos) => await Task.Yield();

        [Rougamo(typeof(ValueOmitArguments))]
        public static async Task<List<string>> ArgumentsAsync(int x)
        {
            await Task.Yield();
            return null;
        }

        [Rougamo<ValueOmitArgumentsButFeature>]
        public static async Task<List<string>> ArgumentsFailedAsync(int x)
        {
            await Task.Yield();
            return null;
        }

        [Rougamo<ValueOmitAll>]
        public async ValueTask<List<string>> AllAsync(int x)
        {
            await Task.Yield();
            return null;
        }

        [Rougamo(typeof(ValueOmitMos))]
        public IEnumerable<string> Iterator(List<string> executedMos)
        {
            yield return string.Empty;
            yield return null;
        }

        [Rougamo<ValueOmitMos>]
        public async IAsyncEnumerable<string> IteratorAsync(List<string> executedMos)
        {
            await Task.Yield();
            yield return null;
            yield return string.Empty;
        }

        [Rougamo<ClassOmitMos>]
        public void Cls(List<string> executedMos)
        {

        }

        [ClassOmitMos]
        public async Task ClsAsync(List<string> executedMos) => await Task.Yield();
    }
}
