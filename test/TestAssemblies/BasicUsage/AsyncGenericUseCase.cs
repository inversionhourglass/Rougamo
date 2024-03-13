using BasicUsage.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class AsyncGenericUseCase<T>
    {
        //[Async]
        //public async Task<T> First(List<T> items)
        //{
        //    await Task.Yield();
        //    return items.FirstOrDefault();
        //}

        [Async]
        public async ValueTask<(T, TT)> Last<TT>(List<T> list1, List<TT> list2)
        {
            await Task.Yield();
            return (list1.Last(), list2.Last());
        }
    }
}
