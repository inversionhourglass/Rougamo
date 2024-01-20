using BasicUsage.Mos;
using Rougamo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class RougamoUsage : IRougamo<Mo>, IRougamo<ValueMo>
    {
        public void M(List<string> executedMos) { }

        public async Task MAsync(List<string> executedMos) => await Task.Yield();
    }
}
