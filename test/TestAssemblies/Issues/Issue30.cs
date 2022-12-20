using Issues.Attributes;
using System.Threading.Tasks;

namespace Issues
{
    public class Issue30
    {
        [_30_]
        public static async Task<string> Test(string arg1)
        {
            await Task.Yield();
            return arg1.Trim();
        }
    }
}
