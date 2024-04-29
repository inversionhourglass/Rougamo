using Issues.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Issues
{
    public class Issue60
    {
        [_60_]
        public async CompletedTask Async(List<string> items)
        {
            await default(CompletedTask);
        }

        [_60_]
        public async CompletedTask AsyncException(List<string> items)
        {
            await default(CompletedTask);
            throw new Exception();
        }

        public class MethodBuilder
        {
            public CompletedTask Task => default;

            public static MethodBuilder Create() => default;

            public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => stateMachine.MoveNext();

            public void SetStateMachine(IAsyncStateMachine stateMachine) { }

            public void SetResult() { }

            public void SetException(Exception exception) { }

            public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine { }

            public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine { }
        }

        [AsyncMethodBuilder(typeof(MethodBuilder))]
        public struct CompletedTask
        {
            public CompletedTaskAwaiter GetAwaiter() => default;
        }

        public class CompletedTaskAwaiter : INotifyCompletion
        {
            public bool IsCompleted => true;

            public void GetResult() { }

            public void OnCompleted(Action continuation) { }
        }
    }
}
