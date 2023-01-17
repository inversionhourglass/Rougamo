using ExUsage.Attributes;
using System;
using System.Threading.Tasks;

namespace ExUsage
{
    public class MultiApplyCase
    {
        [MultiApply(1)]
        [MultiApply(2)]
        public async Task<int> OkAsync()
        {
            await Task.Yield();
            return 0;
        }

        [MultiApply(1)]
        [MultiApply(2)]
        public Task<int> Ok()
        {
            return Task.FromResult(0);
        }

        [MultiApply(1)]
        [MultiApply(2)]
        public async Task<int> OopsAsync()
        {
            throw new NotImplementedException();
        }

        [MultiApply(1)]
        [MultiApply(2)]
        public Task<int> Oops()
        {
            return Task.Run(Throws);

            int Throws() => throw new NotImplementedException();
        }
    }
}
