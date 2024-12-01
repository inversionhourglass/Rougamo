using BasicUsage.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class LifetimeUseCase
    {
        [Singleton]
        public void Singleton(List<object> list)
        {
        }

        [Singleton]
        public void SingletonNested(List<object> list)
        {
            SingletonNestedCore(list);
        }

        [Singleton]
        private void SingletonNestedCore(List<object> list)
        {
        }

        [Singleton]
        public async Task SingletonAsync(List<object> list)
        {
            await Task.CompletedTask;
        }

        [Singleton]
        public async Task SingletonNestedAsync(List<object> list)
        {
            await SingletonNestedCoreAsync(list);
        }

        [Singleton]
        private async Task SingletonNestedCoreAsync(List<object> list)
        {
            await Task.CompletedTask;
        }

        [Singleton]
        public IEnumerable<int> SingletonIterator(List<object> list)
        {
            yield return 1;
        }

        [Singleton]
        public IEnumerable<int> SingletonNestedIterator(List<object> list)
        {
            foreach (var item in SingletonNestedIteratorCore(list))
            {
                yield return item;
            }
        }

        [Singleton]
        private IEnumerable<int> SingletonNestedIteratorCore(List<object> list)
        {
            yield return 1;
        }

        [Singleton]
        public async IAsyncEnumerable<int> SingletonAiterator(List<object> list)
        {
            await Task.Yield();
            yield return 1;
        }

        [Singleton]
        public async IAsyncEnumerable<int> SingletonNestedAiterator(List<object> list)
        {
            await foreach (var item in SingletonNestedAiteratorCore(list))
            {
                yield return item;
            }
        }

        [Singleton]
        private async IAsyncEnumerable<int> SingletonNestedAiteratorCore(List<object> list)
        {
            await Task.Yield();
            yield return 1;
        }

        [Pooled]
        public void Pooled(List<object> list)
        {
        }

        [Pooled]
        public void PooledNested(List<object> list)
        {
            PooledNestedCore(list);
        }

        [Pooled]
        private void PooledNestedCore(List<object> list)
        {
        }

        [Pooled]
        public async Task PooledAsync(List<object> list)
        {
            await Task.CompletedTask;
        }

        [Pooled]
        public async Task PooledNestedAsync(List<object> list)
        {
            await PooledNestedCoreAsync(list);
        }

        [Pooled]
        private async Task PooledNestedCoreAsync(List<object> list)
        {
            await Task.CompletedTask;
        }

        [Pooled]
        public IEnumerable<int> PooledIterator(List<object> list)
        {
            yield return 1;
        }

        [Pooled]
        public IEnumerable<int> PooledNestedIterator(List<object> list)
        {
            foreach (var item in PooledNestedIteratorCore(list))
            {
                yield return item;
            }
        }

        [Pooled]
        private IEnumerable<int> PooledNestedIteratorCore(List<object> list)
        {
            yield return 1;
        }

        [Pooled]
        public async IAsyncEnumerable<int> PooledAiterator(List<object> list)
        {
            await Task.Yield();
            yield return 1;
        }

        [Pooled]
        public async IAsyncEnumerable<int> PooledNestedAiterator(List<object> list)
        {
            await foreach (var item in PooledNestedAiteratorCore(list))
            {
                yield return item;
            }
        }

        [Pooled]
        private async IAsyncEnumerable<int> PooledNestedAiteratorCore(List<object> list)
        {
            await Task.Yield();
            yield return 1;
        }

        [Pooled<int>(Value = 1)]
        public void GenericPooled(List<object> list)
        {
        }

        [Pooled<int>(Value = 2)]
        public async Task GenericPooledAsync(List<object> list)
        {
            await Task.Yield();
        }

        [Transient]
        public void Transient(List<object> list)
        {
        }

        [Transient]
        public void TransientNested(List<object> list)
        {
            TransientNestedCore(list);
        }

        [Transient]
        private void TransientNestedCore(List<object> list)
        {
        }

        [Transient]
        public async Task TransientAsync(List<object> list)
        {
            await Task.CompletedTask;
        }

        [Transient]
        public async Task TransientNestedAsync(List<object> list)
        {
            await TransientNestedCoreAsync(list);
        }

        [Transient]
        private async Task TransientNestedCoreAsync(List<object> list)
        {
            await Task.CompletedTask;
        }

        [Transient]
        public IEnumerable<int> TransientIterator(List<object> list)
        {
            yield return 1;
        }

        [Transient]
        public IEnumerable<int> TransientNestedIterator(List<object> list)
        {
            foreach (var item in TransientNestedIteratorCore(list))
            {
                yield return item;
            }
        }

        [Transient]
        private IEnumerable<int> TransientNestedIteratorCore(List<object> list)
        {
            yield return 1;
        }

        [Transient]
        public async IAsyncEnumerable<int> TransientAiterator(List<object> list)
        {
            await Task.Yield();
            yield return 1;
        }

        [Transient]
        public async IAsyncEnumerable<int> TransientNestedAiterator(List<object> list)
        {
            await foreach (var item in TransientNestedAiteratorCore(list))
            {
                yield return item;
            }
        }

        [Transient]
        private async IAsyncEnumerable<int> TransientNestedAiteratorCore(List<object> list)
        {
            await Task.Yield();
            yield return 1;
        }
    }
}
