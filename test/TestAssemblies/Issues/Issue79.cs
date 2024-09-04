using Issues.Attributes;
using System;

namespace Issues
{
    [_79_]
    public class Issue79
    {
        public bool M()
        {
            var random = new Random();
            var l = new Issue79();
            return random.Next() == l.GetHashCode();
        }
    }
}
