using BasicUsage.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class FreshArguments
    {
        [FreshArgs]
        public Tuple<object[], object[]> FreshTest(int x, out int y)
        {
            x++;
            y = -1;
            return null;
        }

        [FreshArgs]
        public async Task<Tuple<object[], object[]>> FreshTestAsync(string s, List<int> ls)
        {
            s += s;
            await Task.Yield();
            ls = [1, 2, 3];
            return null;
        }

        [NonFreshArgs]
        public Tuple<object[], object[]> NonFreshTest(int x, out int y)
        {
            x++;
            y = -1;
            return null;
        }

        [NonFreshArgs]
        public async Task<Tuple<object[], object[]>> NonFreshTestAsync(string s, List<int> ls)
        {
            s += s;
            await Task.Yield();
            ls = [1, 2, 3];
            return null;
        }
    }
}
