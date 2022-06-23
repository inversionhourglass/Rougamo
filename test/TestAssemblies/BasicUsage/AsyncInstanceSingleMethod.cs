using BasicUsage.Attributes;
using System;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class AsyncInstanceSingleMethod : MoDataContainer
    {
        [OnEntry]
        public async Task VoidAsync()
        {
            await Task.Yield();
        }

        [OnEntry]
#if NET461 || NET6
        public async Task<string> EntryAsync(int number, string str, object[] array)
#else
        public async ValueTask<string> EntryAsync(int number, string str, object[] array)
#endif
        {
            await Task.Yield();
            return $"{number}|{str}|{string.Join(",", array)}";
        }

        [OnException]
#if NET461 || NET6
        public async Task<string> ExceptionAsync()
#else
        public async ValueTask<string> ExceptionAsync()
#endif
        {
            await Task.Yield();
            throw new InvalidOperationException(nameof(ExceptionAsync));
        }

        [OnSuccess]
#if NET461 || NET6
        public async Task<int> SuccessAsync()
#else
        public async ValueTask<int> SuccessAsync()
#endif
        {
            await Task.Yield();
            return nameof(SuccessAsync).Length;
        }

        [OnExit]
#if NET461 || NET6
        public async Task<string> ExitWithExceptionAsync()
#else
        public async ValueTask<string> ExitWithExceptionAsync()
#endif
        {
            await Task.Yield();
            throw new InvalidOperationException(nameof(ExitWithExceptionAsync));
        }

        [OnExit]
#if NET461 || NET6
        public async Task<string> ExitWithSuccessAsync()
#else
        public async ValueTask<string> ExitWithSuccessAsync()
#endif
        {
            await Task.Yield();
            return nameof(ExitWithSuccessAsync);
        }
    }
}
