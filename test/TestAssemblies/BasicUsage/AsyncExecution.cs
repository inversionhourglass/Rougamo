using BasicUsage.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class AsyncExecution
    {
        [Async]
        public async Task Void1(List<string> datas)
        {
            datas.Add(nameof(Void1));
            await Task.Yield();
        }

        [Async]
        public async Task Void2(List<string> datas)
        {
            await Task.Yield();
            datas.Add(nameof(Void2));
        }

        [Async]
        public async Task VoidThrows1(List<string> datas)
        {
            datas.Add(nameof(VoidThrows1));
            await Task.Yield();
            throw new InvalidOperationException();
        }

        [Async]
        public async Task VoidThrows2(List<string> datas)
        {
            await Task.Yield();
            datas.Add(nameof(VoidThrows2));
            throw new InvalidOperationException();
        }

        [Async]
        public async Task Empty(List<string> datas)
        {

        }

        [AsyncEntryModifier]
        public async Task EmptyReplaceOnEntry(List<string> datas)
        {

        }

        [AsyncEntryModifier]
        public async Task<string> ReplaceOnEntry(List<string> datas)
        {
            datas.Add(nameof(ReplaceOnEntry));
            await Task.Yield();
            return null;
        }

        [AsyncExceptionModifier]
        public async Task<string> ReplaceOnException(List<string> datas)
        {
            datas.Add(nameof(ReplaceOnException));
            await Task.Yield();
            throw new InvalidOperationException();
        }

        [AsyncSuccessModifier]
        public async Task<string> ReplaceOnSuccess(List<string> datas)
        {
            datas.Add(nameof(ReplaceOnSuccess));
            await Task.Yield();
            return null;
        }
    }
}
