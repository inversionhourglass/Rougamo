using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace System
{
    /// <summary>
    /// <see cref="Exception"/> extension methods
    /// </summary>
    public static class ExceptionExtensions
    {
        // Exception._stackTraceString
        private static readonly Action<Exception, string>? _ExceptionSetStackTraceString;
        // StackTrace._numOfFrames
        private static readonly Action<StackTrace, int>? _StackTraceSetNumOfFrames;
        // StackTrace._stackFrames
        private static readonly Action<StackTrace, StackFrame[]>? _StackTraceSetStackFrames;

        static ExceptionExtensions()
        {
            _ExceptionSetStackTraceString = ResolveExceptionSetStackTraceString();
            _StackTraceSetNumOfFrames = ResolveStackTraceSetNumOfFrames();
            _StackTraceSetStackFrames = ResolveStackTraceSetStackFrames();
        }

        /// <summary>
        /// <see cref="Exception"/> ToString without Rougamo stack frames
        /// </summary>
        public static string ToNonRougamoString(this Exception exception)
        {
            if (_ExceptionSetStackTraceString != null && _StackTraceSetNumOfFrames != null && _StackTraceSetStackFrames != null)
            {
                var pureStackTrace = exception.GetNonRougamoStackTrace();
                _ExceptionSetStackTraceString(exception, pureStackTrace);
            }

            return exception.ToString();
        }

        /// <summary>
        /// Get the stack trace without Rougamo stack frames from <paramref name="exception"/>
        /// </summary>
        public static string GetNonRougamoStackTrace(this Exception exception)
        {
            if (_StackTraceSetNumOfFrames == null || _StackTraceSetStackFrames == null) return exception.StackTrace;

            var stackTrace = new StackTrace(exception, true);
            var filteredFrames = new List<StackFrame>();
            var frames = stackTrace.GetFrames();

            MethodBase? syncMethod = null;
            string? asyncMethodName = null;
            var asyncState = AsyncState.Inactive;
            for (var i = 0; i < frames.Length; i++)
            {
                var frame = frames[i];
                if (frame == null) continue;

                if (!frame.HasMethod())
                {
                    syncMethod = null;
                    asyncMethodName = null;
                    filteredFrames.Add(frame);
                    continue;
                }

                var method = frame.GetMethod()!;
                if (method.Name.StartsWith("$Rougamo_"))
                {
                    syncMethod = method;
                    asyncMethodName = null;
                }
                else if (method.DeclaringType != null && method.DeclaringType.Name.StartsWith("<$Rougamo_"))
                {
                    var typeName = method.DeclaringType.Name;
                    syncMethod = null;
                    asyncMethodName = typeName.Substring(10, typeName.IndexOf('>') - 10);
                    asyncState = AsyncState.Inactive;
                }
                else if (method.DeclaringType != null && method.DeclaringType.Name.Contains(">r__"))
                {
                    var typeName = method.DeclaringType.Name;
                    syncMethod = null;
                    asyncMethodName = typeName.Substring(1, typeName.IndexOf('>') - 1);
                    asyncState = AsyncState.MoveNext;
                    continue;
                }
                else if (syncMethod != null)
                {
                    if (method.Name.Length + 9 == syncMethod.Name.Length && syncMethod.Name.EndsWith(method.Name))
                    {
                        syncMethod = null;
                        continue;
                    }
                    syncMethod = null;
                }
                else if (asyncMethodName != null)
                {
                    if (method.DeclaringType != null)
                    {
                        switch (asyncState)
                        {
                            case AsyncState.Inactive:
                                if (method.Name == "MoveNext" && method.DeclaringType.Name.StartsWith($"<{asyncMethodName}>"))
                                {
                                    asyncState = AsyncState.MoveNext;
                                    continue;
                                }
                                filteredFrames.Add(frame);
                                continue;
                            case AsyncState.MoveNext:
                                if (method.Name == "Throw" && method.DeclaringType == typeof(ExceptionDispatchInfo))
                                {
                                    asyncState = AsyncState.Throw;
                                    continue;
                                }
                                break;
                            case AsyncState.Throw:
                                if (method.Name == "MoveNext" && method.DeclaringType.Name.StartsWith($"<{asyncMethodName}>"))
                                {
                                    asyncState = AsyncState.MoveNext;
                                    continue;
                                }
                                if (method.Name == "ThrowForNonSuccess")
                                {
                                    asyncState = AsyncState.ThrowNonSuccess;
                                    continue;
                                }
                                if (method.Name == "HandleNonSuccessAndDebuggerNotification")
                                {
                                    asyncState = AsyncState.Notification;
                                    continue;
                                }
                                break;
                            case AsyncState.ThrowNonSuccess:
                                if (method.Name == "HandleNonSuccessAndDebuggerNotification")
                                {
                                    asyncState = AsyncState.Notification;
                                    continue;
                                }
                                break;
                            case AsyncState.Notification:
                                if (method.Name == "GetResult")
                                {
                                    asyncState = AsyncState.GetResult;
                                    continue;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    asyncMethodName = null;
                    asyncState = AsyncState.Inactive;
                }
                filteredFrames.Add(frame);
            }

            _StackTraceSetNumOfFrames(stackTrace, filteredFrames.Count);
            _StackTraceSetStackFrames(stackTrace, filteredFrames.ToArray());

            return stackTrace.ToString();
        }

        private static Action<Exception, string>? ResolveExceptionSetStackTraceString()
        {
            var field = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) return null;

            var exceptionParam = Expression.Parameter(typeof(Exception), "exception");
            var stackTraceStringParam = Expression.Parameter(typeof(string), "stackTraceString");

            var fieldAccess = Expression.Field(exceptionParam, field);
            var assign = Expression.Assign(fieldAccess, stackTraceStringParam);

            return Expression.Lambda<Action<Exception, string>>(assign, exceptionParam, stackTraceStringParam).Compile();
        }

        private static Action<StackTrace, int>? ResolveStackTraceSetNumOfFrames()
        {
            var tStackTrace = typeof(StackTrace);
            var fields = tStackTrace.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.FieldType == typeof(int) && f.Name.IndexOf("numOfFrames", StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
            if (fields.Length != 1) return null;

            var stackTraceParam = Expression.Parameter(typeof(StackTrace), "stackTrace");
            var numOfFramesParam = Expression.Parameter(typeof(int), "numOfFrames");

            var fieldAccess = Expression.Field(stackTraceParam, fields[0]);
            var assign = Expression.Assign(fieldAccess, numOfFramesParam);

            return Expression.Lambda<Action<StackTrace, int>>(assign, stackTraceParam, numOfFramesParam).Compile();
        }

        private static Action<StackTrace, StackFrame[]>? ResolveStackTraceSetStackFrames()
        {
            var tStackTrace = typeof(StackTrace);
            var fields = tStackTrace.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.FieldType == typeof(StackFrame[])).ToArray();
            if (fields.Length != 1) return null;

            var stackTraceParam = Expression.Parameter(typeof(StackTrace), "stackTrace");
            var stackFramesParam = Expression.Parameter(typeof(StackFrame[]), "stackFrames");

            var fieldAccess = Expression.Field(stackTraceParam, fields[0]);
            var assign = Expression.Assign(fieldAccess, stackFramesParam);

            return Expression.Lambda<Action<StackTrace, StackFrame[]>>(assign, stackTraceParam, stackFramesParam).Compile();
        }

        /**
         * 不同的.NET版本在异步方法中抛出异常时的调用栈可能不同
         * [net461,        net48        ] MoveNext -> Throw -> Notification -> GetResult
         * [netcoreapp2.0, netcoreapp2.1] MoveNext -> Throw -> Notification
         * [netcoreapp3.1, net6.0       ] MoveNext -> Throw -> ThrowNonSuccess -> Notification -> GetResult
         * [ent7.0,        net8.0       ] MoveNext -> Throw -> ThrowNonSuccess -> Notification
         * 
         * 如果OnException或OnExit有异步执行的，还会而外产生两次堆栈：MoveNext -> Throw
         * [net461,        net48        ] MoveNext -> Throw -> MoveNext -> Throw -> Notification -> GetResult
         * [netcoreapp2.0, netcoreapp2.1] MoveNext -> Throw -> MoveNext -> Throw -> Notification
         * [netcoreapp3.1, net6.0       ] MoveNext -> Throw -> MoveNext -> Throw -> ThrowNonSuccess -> Notification -> GetResult
         * [ent7.0,        net8.0       ] MoveNext -> Throw -> MoveNext -> Throw -> ThrowNonSuccess -> Notification
         */
        enum AsyncState
        {
            Inactive,
            MoveNext,        // StateMachine.MoveNext()
            Throw,           // ExceptionDispatchInfo.Throw()
            ThrowNonSuccess, // <TaskAwaiter>.ThrowForNonSuccess(Task)
            Notification,    // <TaskAwaiter>.HandleNonSuccessAndDebuggerNotification(Task)
            GetResult        // <TaskAwaiter>.GetResult()
        }
    }
}
