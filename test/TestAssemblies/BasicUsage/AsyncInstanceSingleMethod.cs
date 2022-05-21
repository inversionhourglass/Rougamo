using BasicUsage.Attributes;
using System;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class AsyncInstanceSingleMethod : MoDataContainer
    {
        [OnEntry]
        public async Task<string> EntryAsync(int number, string str, object[] array)
        {
            await Task.Yield();
            return $"{number}|{str}|{string.Join(",", array)}";
        }

        [OnException]
        public async Task<string> ExceptionAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException(nameof(ExceptionAsync));
        }

        [OnSuccess]
        public async Task<int> SuccessAsync()
        {
            await Task.Yield();
            return nameof(SuccessAsync).Length;
        }

        [OnExit]
        public async Task<string> ExitWithExceptionAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException(nameof(ExitWithExceptionAsync));
        }

        [OnExit]
        public async Task<string> ExitWithSuccessAsync()
        {
            await Task.Yield();
            return nameof(ExitWithSuccessAsync);
        }
    }
}
