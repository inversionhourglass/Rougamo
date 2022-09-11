using ExUsage.Attributes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ExUsage
{
    public class ExecutionOrder
    {
        [Ordering("1")]
        [Ordering("2")]
        public int Sync(List<string> items)
        {
            Thread.Yield();
            return 1;
        }

        [Ordering("1")]
        [Ordering("2")]
        public async Task Async(List<string> items)
        {
            await Task.Yield();
        }

        [Ordering("1")]
        [Ordering("2")]
        public Task<double> NonAsync(List<string> items)
        {
            return Task.FromResult(Math.PI);
        }

        [Ordering("1")]
        [Ordering("2")]
        public int SyncException(List<string> items)
        {
            Thread.Yield();
            throw new InvalidOperationException(nameof(SyncException));
        }

        [Ordering("1")]
        [Ordering("2")]
        public async Task AsyncException1(List<string> items)
        {
            if (items != null) throw new InvalidOperationException(nameof(AsyncException1));

            await Task.Yield();
        }

        [Ordering("1")]
        [Ordering("2")]
        public async Task AsyncException2(List<string> items)
        {
            await Task.Yield();
            throw new InvalidOperationException(nameof(AsyncException2));
        }

        [Ordering("1")]
        [Ordering("2")]
        public Task<double> NonAsyncException1(List<string> items)
        {
            if (items != null) throw new InvalidOperationException(nameof(NonAsyncException1));

            return Task.FromResult(Math.PI);
        }

        [Ordering("1")]
        [Ordering("2")]
        public Task<double> NonAsyncException2(List<string> items)
        {
            return Task.Run(double() => throw new InvalidOperationException(nameof(NonAsyncException1)));
        }
    }
}
