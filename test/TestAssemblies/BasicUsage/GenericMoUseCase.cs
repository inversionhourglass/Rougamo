using BasicUsage.Mos;
using Rougamo;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: Rougamo<ValueMo3>]

namespace BasicUsage
{
    [Rougamo<ValueMo>]
    public class GenericMoUseCase : IRougamo<ValueMo1>
    {
        [Rougamo<ValueMo2>]
        public void M(List<string> executedMos) { }

        [Rougamo<Mo>]
        public async ValueTask MAsync(List<string> executedMos) => await Task.Yield();
    }
}
