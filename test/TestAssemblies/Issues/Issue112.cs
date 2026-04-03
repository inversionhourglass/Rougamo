using Issues.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Issues;

public class Issue112
{
    [_112_]
    public async NoAwaiterTask NoAwaiterAsync(List<string> logs)
    {
    }

    [AsyncMethodBuilder(typeof(NoAwaiterTaskMethodBuilder))]
    public readonly struct NoAwaiterTask
    {
    }

    public struct NoAwaiterTaskMethodBuilder
    {
        public NoAwaiterTask Task => new();

        public static NoAwaiterTaskMethodBuilder Create() => new();

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        public void SetResult()
        {
        }

        public void SetException(Exception exception)
        {
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
        }
    }
}
