using BasicUsage.Mos;
using Rougamo;
using System.Collections.Generic;
using System.Threading.Tasks;

#if MONO
[assembly: Rougamo(typeof(ValueMo3))]
#else
[assembly: Rougamo<ValueMo3>]
#endif

namespace BasicUsage
{
#if MONO
    [Rougamo(typeof(ValueMo))]
#else
    [Rougamo<ValueMo>]
#endif
    public class GenericMoUseCase : IRougamo<ValueMo1>
    {
#if MONO
        [Rougamo(typeof(ValueMo2))]
#else
        [Rougamo<ValueMo2>]
#endif
        public void M(List<string> executedMos) { }

#if MONO
        [Rougamo(typeof(BasicUsage.Mos.Mo))]
#else
        [Rougamo<BasicUsage.Mos.Mo>]
#endif
        public async ValueTask MAsync(List<string> executedMos) => await Task.Yield();
    }
}
