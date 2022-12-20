using Issues.Attributes;
using System.Threading.Tasks;

namespace Issues
{
    public class Issue29
    {
        [_29_]
        public async Task Test(string unused)
        {
            await Task.Yield();
        }
    }
}
