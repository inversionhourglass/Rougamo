using Issues.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Issues
{
    public class Issue60
    {
        [_60_]
        public async ValueTask Async(List<string> items)
        {
            await new CompletedTask();
        }

        [_60_]
        public async ValueTask AsyncException(List<string> items)
        {
            await new CompletedTask();
            throw new Exception();
        }

#if NET48 || NET7_0_OR_GREATER
        public class MethodBuilder
#else
        public struct MethodBuilder
#endif
        {
            public CompletedTask Task => new();

            public static MethodBuilder Create() => new();

            public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => stateMachine.MoveNext();

            public void SetStateMachine(IAsyncStateMachine stateMachine) { }

            public void SetResult() { }

            public void SetException(Exception exception) { }

            public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine { }

            public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine { }
        }

        [AsyncMethodBuilder(typeof(MethodBuilder))]
#if NET461 || NETCOREAPP3_1 || NET8_0_OR_GREATER
        public class CompletedTask
#else
        public struct CompletedTask
#endif
        {
            public CompletedTaskAwaiter GetAwaiter() => new ();
        }

#if NET6_0_OR_GREATER
        public class CompletedTaskAwaiter : ICriticalNotifyCompletion
#else
        public struct CompletedTaskAwaiter : ICriticalNotifyCompletion
#endif
        {
            public bool IsCompleted => true;

            public void GetResult() { }

            public void OnCompleted(Action continuation) { }

            public void UnsafeOnCompleted(Action continuation) { }
        }
    }
}
