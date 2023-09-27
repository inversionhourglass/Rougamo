using BasicUsage.Attributes;
using Rougamo;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BasicUsage
{
    [CtorInit]
    [CctorInit]
    public class SyncExecution
    {
        [CcctorInit]
        public SyncExecution()
        {

        }

        [CcctorInit]
        static SyncExecution()
        {

        }

        [IgnoreMo]
        public string InstanceProp1 { get; set; }

        [IgnoreMo]
        public string InstanceProp2 { get; set;}

        [IgnoreMo]
        public static string StaticProp1 { get; set; }

        [IgnoreMo]
        public static string StaticProp2 { get; set; }

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

        [Sync]
        public void Empty(List<string> datas)
        {

        }

        [SyncEntryModifier]
        public void EmptyReplaceOnEntry(List<string> datas)
        {

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

        [Retry]
        public void RetryException(List<int> datas)
        {
            datas.Add(1);
            throw new Exception();
        }

        [Retry]
        public void RetrySuccess(List<int> datas)
        {
            datas.Add(1);
        }

        [IgnoreMo]
        public object Call(string methodName, object[] args, params Type[] genericTypes)
        {
            var method = GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            if (method == null) throw new Exception($"Method({methodName}) not found");

            if (genericTypes.Length > 0) method = method.MakeGenericMethod(genericTypes);

            var caller = method.IsStatic ? null : this;
            return method.Invoke(caller, args ?? Array.Empty<object>());
        }
    }
}
