using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Rougamo
{
    /// <summary>
    /// Object pool implementation.
    /// </summary>
    public static class RougamoPool<T> where T : class, new()
    {
        private static readonly int _MaxCapacity;
        private static readonly ConcurrentQueue<T> _Items = new();
        private static T? _FastItem;
        private static int _NumItems;

        /// <summary>
        /// </summary>
        static RougamoPool()
        {
            var formatName = typeof(T).FullName!.Replace('.', '_');
            var envMaximumRetained = Environment.GetEnvironmentVariable($"NET_ROUGAMO_POOL_MAX_RETAIN_{formatName}");
            envMaximumRetained ??= Environment.GetEnvironmentVariable("NET_ROUGAMO_POOL_MAX_RETAIN");

            if (envMaximumRetained == null || !int.TryParse(envMaximumRetained, out var maximumRetained))
            {
                maximumRetained = Environment.ProcessorCount * 2;
            }

            _MaxCapacity = maximumRetained - 1;
        }

        /// <summary>
        /// Get <typeparamref name="T"/> instance from the pool.
        /// </summary>
        public static T Get()
        {
            var item = _FastItem;
            if (item == null || Interlocked.CompareExchange(ref _FastItem, null, item) != item)
            {
                if (_Items.TryDequeue(out item))
                {
                    Interlocked.Decrement(ref _NumItems);
                    return item;
                }

                return new();
            }

            return item;
        }

        /// <summary>
        /// Return <paramref name="value"/> back to the pool.
        /// </summary>
        public static void Return(T value)
        {
            if (value is IResettable resettable && !resettable.TryReset()) return;

            if (_FastItem != null || Interlocked.CompareExchange(ref _FastItem, value, null) != null)
            {
                if (Interlocked.Increment(ref _NumItems) <= _MaxCapacity)
                {
                    _Items.Enqueue(value);
                }
                else
                {
                    Interlocked.Decrement(ref _NumItems);
                }
            }
        }
    }
}
