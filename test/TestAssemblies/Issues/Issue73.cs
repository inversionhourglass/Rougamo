using Issues.Attributes;
using System;
using System.Threading.Tasks;

namespace Issues
{
    public class Issue73
    {
        public class Base<T> where T : IComparable
        {
            public virtual async Task MAsync()
            {
                await Task.Yield();
            }
        }

        public class Cls<T> : Base<T> where T : IComparable
        {
            [_73_]
            public override async Task MAsync()
            {
                await base.MAsync();
            }
        }
    }
}
