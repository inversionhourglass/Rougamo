using Issues.Attributes;
using System.Collections.Generic;

namespace Issues
{
    [_66_]
    public class Issue66<T1, T2>
    {
        public class Inner
        {

        }

        public void M()
        {
            var list = new List<Issue66<T1, T2>.Inner>();
            list.ForEach(x => MM());
        }

        public void MM() { }
    }
}
