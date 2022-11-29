using Issues.Attributes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Issues
{
    public class Issue25
    {
        [_25_]
        public T GenericMethod<T>() where T : class, IDisposable
        {
            Thread.Yield();

            return null;
        }

        [_25_]
        public async Task<T> AsyncGenericMethod<T>() where T : class, IDisposable
        {
            await Task.Yield();

            return null;
        }
    }
}
