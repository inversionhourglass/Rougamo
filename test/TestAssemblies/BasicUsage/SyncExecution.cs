using BasicUsage.Attributes;
using System;
using System.Collections.Generic;

namespace BasicUsage
{
    public class SyncExecution
    {
        [Sync]
        public void Void(List<string> datas)
        {
            datas.Add(nameof(Void));
        }

        [Sync]
        public void VoidThrows(List<string> datas)
        {
            datas.Add(nameof(VoidThrows));
            throw new InvalidOperationException();
        }

        [SyncEntryModifier]
        public string ReplaceOnEntry(List<string> datas)
        {
            datas.Add(nameof(ReplaceOnEntry));
            return null;
        }

        [SyncExceptionModifier]
        public string ReplaceOnException(List<string> datas)
        {
            datas.Add(nameof(ReplaceOnException));
            throw new InvalidOperationException();
        }

        [SyncSuccessModifier]
        public string ReplaceOnSuccess(List<string> datas)
        {
            datas.Add(nameof(ReplaceOnSuccess));
            return null;
        }
    }
}
