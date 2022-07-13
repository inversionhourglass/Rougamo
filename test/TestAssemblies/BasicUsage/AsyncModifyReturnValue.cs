using BasicUsage.Attributes;
using System;
using System.Threading.Tasks;

namespace BasicUsage
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public class AsyncModifyReturnValue : MoDataContainer
    {
        [ExceptionHandle]
#if NET461 || NET6
        public async Task<string> ExceptionAsync(bool throwException = true)
#else
        public async ValueTask<string> ExceptionAsync(bool throwException = true)
#endif
        {
            await Task.Yield();
            if (throwException) throw new InvalidOperationException(nameof(ExceptionAsync));

            return Guid.NewGuid().ToString();
        }

        [ExceptionHandle]
#if NET461 || NET6
        public async Task<int> ExceptionWithUnboxAsync(bool throwException = true)
#else
        public async ValueTask<int> ExceptionWithUnboxAsync(bool throwException = true)
#endif
        {
            await Task.Yield();
            if (throwException) throw new InvalidOperationException(nameof(ExceptionWithUnboxAsync));

            return GetHashCode();
        }

        [ExceptionHandle]
#if NET461 || NET6
        public async Task<double> ExceptionUnhandledAsync(bool throwException = true)
#else
        public async ValueTask<double> ExceptionUnhandledAsync(bool throwException = true)
#endif
        {
            await Task.Yield();
            if (throwException) throw new InvalidOperationException(nameof(ExceptionUnhandledAsync));

            return GetHashCode() * 1.0 / nameof(ExceptionUnhandledAsync).Length;
        }

        [ReturnValueReplace]
#if NET461 || NET6
        public async Task<string> SucceededAsync(object[] args)
#else
        public async ValueTask<string> SucceededAsync(object[] args)
#endif
        {
            await Task.Yield();
            return Guid.NewGuid().ToString() + "|" + string.Join("|", args);
        }

        [ReturnValueReplace]
#if NET461 || NET6
        public async Task<int> SucceededWithUnboxAsync()
#else
        public async ValueTask<int> SucceededWithUnboxAsync()
#endif
        {
            await Task.Yield();
            return int.MaxValue % nameof(SucceededWithUnboxAsync).Length;
        }

        [ReturnValueReplace]
#if NET461 || NET6
        public async Task<double> SucceededUnrecognizedAsync()
#else
        public async ValueTask<double> SucceededUnrecognizedAsync()
#endif
        {
            await Task.Yield();
            return int.MaxValue * 1.0 / nameof(SucceededUnrecognizedAsync).Length;
        }

        [ReplaceValueOnEntry]
#if NET461 || NET6
        public async Task<string[]> CachedArrayAsync()
#else
        public async ValueTask<string[]> CachedArrayAsync()
#endif
        {
            return new string[0];
        }

        [ReplaceValueOnEntry]
#if NET461 || NET6
        public async Task<string[]> CachedEvenThrowsAsync()
#else
        public async ValueTask<string[]> CachedEvenThrowsAsync()
#endif
        {
            throw new NullReferenceException();
        }

        [ReplaceValueOnEntry]
#if NET461 || NET6
        public async Task<long> TryReplaceLongToNullAsync()
#else
        public async ValueTask<long> TryReplaceLongToNullAsync()
#endif
        {
            await Task.Yield();
            return 123;
        }

        [ReplaceValueOnEntry]
#if NET461 || NET6
        public async Task<long?> TryReplaceNullableToNullAsync()
#else
        public async ValueTask<long?> TryReplaceNullableToNullAsync()
#endif
        {
            await Task.Yield();
            return 321;
        }
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
