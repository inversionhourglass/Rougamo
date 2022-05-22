using BasicUsage.Attributes;
using System;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class AsyncModifyReturnValue : MoDataContainer
    {
        [ExceptionHandle]
        public async Task<string> ExceptionAsync(bool throwException = true)
        {
            await Task.Yield();
            if (throwException) throw new InvalidOperationException(nameof(ExceptionAsync));

            return Guid.NewGuid().ToString();
        }

        [ExceptionHandle]
        public async Task<int> ExceptionWithUnboxAsync(bool throwException = true)
        {
            await Task.Yield();
            if (throwException) throw new InvalidOperationException(nameof(ExceptionWithUnboxAsync));

            return GetHashCode();
        }

        [ExceptionHandle]
        public async Task<double> ExceptionUnhandledAsync(bool throwException = true)
        {
            await Task.Yield();
            if (throwException) throw new InvalidOperationException(nameof(ExceptionUnhandledAsync));

            return GetHashCode() * 1.0 / nameof(ExceptionUnhandledAsync).Length;
        }

        [ReturnValueReplace]
        public async Task<string> SucceededAsync(object[] args)
        {
            await Task.Yield();
            return Guid.NewGuid().ToString() + "|" + string.Join("|", args);
        }

        [ReturnValueReplace]
        public async Task<int> SucceededWithUnboxAsync()
        {
            await Task.Yield();
            return int.MaxValue % nameof(SucceededWithUnboxAsync).Length;
        }

        [ReturnValueReplace]
        public async Task<double> SucceededUnrecognizedAsync()
        {
            await Task.Yield();
            return int.MaxValue * 1.0 / nameof(SucceededUnrecognizedAsync).Length;
        }
    }
}
