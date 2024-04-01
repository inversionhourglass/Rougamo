using BasicUsage.Attributes;
using System;

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

        [NonFreshArgs]
        public Tuple<object[], object[]> NonFreshTest(int x, out int y)
        {
            x++;
            y = -1;
            return null;
        }
    }
}
