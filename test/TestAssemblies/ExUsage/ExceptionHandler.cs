using ExUsage.Attributes;
using System;
using System.Threading.Tasks;

namespace ExUsage
{
    public class ExceptionHandler
    {
        [ExceptionReplaceDouble]
        public double Sync(double num1, double num2)
        {
            if (num1 + num2 != double.MaxValue - GetType().GetHashCode())
            {
                throw new InvalidOperationException(nameof(Sync));
            }
            return num1 - num2;
        }

        [ExceptionReplaceDouble]
        public async Task<double> Async1()
        {
            if (DateTime.Now != DateTime.MinValue)
            {
                throw new InvalidOperationException(nameof(Async1));
            }
            await Task.Yield();
            return DateTime.Now.Millisecond;
        }

        [ExceptionReplaceDouble]
        public async Task<double> Async2()
        {
            await Task.Yield();
            throw new InvalidOperationException(nameof(Async2));
        }

        [ExceptionReplaceDouble]
        public Task<double> NonAsync1()
        {
            if (DateTime.Now != DateTime.MaxValue)
            {
                throw new InvalidOperationException(nameof(Async1));
            }
            return Task.FromResult(12.34);
        }

        [ExceptionReplaceDouble]
        public Task<double> NonAsync2()
        {
            return Task.Run(double () => throw new InvalidOperationException(nameof(NonAsync2)));
        }

        [ExceptionReplaceDouble]
        public Task<float> UnmatchTypeNonAsync1()
        {
            if (DateTime.Now != DateTime.MaxValue)
            {
                throw new InvalidOperationException(nameof(UnmatchTypeNonAsync1));
            }
            return Task.FromResult(12.34F);
        }

        [ExceptionReplaceDouble]
        public Task<float> UnmatchTypeNonAsync2()
        {
            return Task.Run(float () => throw new InvalidOperationException(nameof(UnmatchTypeNonAsync2)));
        }
    }
}
