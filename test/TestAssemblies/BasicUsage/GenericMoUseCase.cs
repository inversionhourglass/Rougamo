using BasicUsage.Mos;
using Rougamo;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: Mo<ValueMo3>]

namespace BasicUsage
{
    [Mo<ValueMo>]
    public class GenericMoUseCase : IRougamo<ValueMo1>
    {
        [Mo<ValueMo2>]
        public void M(List<string> executedMos) { }

        [Mo<Mo>]
        public async ValueTask MAsync(List<string> executedMos) => await Task.Yield();
    }
}
