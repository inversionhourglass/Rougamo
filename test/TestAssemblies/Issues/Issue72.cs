using Issues.Attributes;
using Rougamo;
using System.Collections.Generic;

namespace Issues
{
    public class Issue72
    {
        [Rougamo<_72_>]
        public int Plus(List<string> logs, int x, int y) => x + y;
    }
}
