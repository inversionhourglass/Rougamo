using Issues.Attributes;
using System.Threading.Tasks;

namespace Issues
{
    public class Issue32
    {
        [_32_]
        public Task<int> WithoutAsync()
        {
            return Task.FromResult(100);
        }

        [_32_]
        public async Task<int> WithAsync()
        {
            await Task.Yield();
            return 100;
        }
    }
}
