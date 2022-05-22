using BasicUsage.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class ModifyIteratorReturnValue : MoDataContainer
    {
        [ReturnValueReplace]
        public IEnumerable<int> Succeed(int count, int min, int max)
        {
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                yield return random.Next(min, max);
            }
        }

        [ExceptionHandle]
        public IEnumerable<int> Exception(int count)
        {
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                if (i == 3) throw new InvalidOperationException(nameof(Exception));
                yield return random.Next();
            }
        }

        [ReturnValueReplace]
        public async IAsyncEnumerable<int> SucceedAsync(int count, int min, int max)
        {
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                await Task.Yield();
                var x = random.Next(min, max);
                if (x < min || x >= max) throw new SystemException("???");
                yield return x;
            }
        }

        [ExceptionHandle]
        public async IAsyncEnumerable<int> ExceptionAsync(int count)
        {
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                await Task.Yield();
                if (i == 3) throw new InvalidOperationException(nameof(Exception));
                yield return random.Next();
            }
        }
    }
}
