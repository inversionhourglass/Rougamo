using BasicUsage.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class FreshArguments
    {
        [FreshArgs]
        public void FreshTest(int x, out int y)
        {
            x++;
            y = -1;
        }

        [FreshArgs]
        public async Task FreshTestAsync(string s, List<int> ls)
        {
            s += s;
            await Task.Yield();
            ls = [1, 2, 3];
        }

        [NonFreshArgs]
        public void NonFreshTest(int x, out int y)
        {
            x++;
            y = -1;
        }

        [NonFreshArgs]
        public async Task NonFreshTestAsync(string s, List<int> ls)
        {
            s += s;
            await Task.Yield();
            ls = [1, 2, 3];
        }
    }
}
