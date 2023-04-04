using Rougamo;
using Rougamo.Context;
using System;

namespace BasicUsage.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SyncFeatureAttribute : MoAttribute
    {
        public static readonly Exception RetryException = new InvalidOperationException();
        public static readonly Exception HandleableException = new ArgumentException();
        public static readonly string ReplaceableReturn = null;
        public static readonly string ReplacedReturn = "null";
        public static readonly string RetryReturn = string.Empty;
        public static readonly string RetriedReturn = "retried";

        public SyncFeatureAttribute(int seed)
        {
            Seed = seed;
        }

        public int Seed { get; }

        public override Feature Features { get; set; } = Feature.Observe;

        public override void OnEntry(MethodContext context)
        {
            var recording = (IRecording)context.Target;
            recording.Recording.Add($"{nameof(OnEntry)}-{Seed}");

            if(context.Arguments.Length == 1 && context.Arguments[0].GetType() == typeof(Guid))
            {
                context.RewriteArguments = true;
                context.Arguments[0] = Guid.Empty;
            }
            else if(context.Arguments.Length > 1)
            {
                context.ReplaceReturnValue(this, ReplacedReturn);
            }
        }

        public override void OnException(MethodContext context)
        {
            var recording = (IRecording)context.Target;
            recording.Recording.Add($"{nameof(OnException)}-{Seed}");

            if (context.Exception == RetryException)
            {
                if (context.RetryCount > 0)
                {
                    context.RetryCount = 0;
                    return;
                }
                context.RetryCount = 1;
            }
            else if (context.Exception == HandleableException)
            {
                context.HandledException(this, ReplacedReturn);
            }
        }

        public override void OnSuccess(MethodContext context)
        {
            var recording = (IRecording)context.Target;
            recording.Recording.Add($"{nameof(OnSuccess)}-{Seed}");

            if (context.ReturnValue is string v1 && v1 == RetryReturn)
            {
                if(context.RetryCount > 0)
                {
                    context.ReplaceReturnValue(this, RetriedReturn);
                    return;
                }
                context.RetryCount = 1;
            }
            else if (context.ReturnValue is string v2 && v2 == ReplaceableReturn)
            {
                context.ReplaceReturnValue(this, ReplacedReturn);
            }
        }

        public override void OnExit(MethodContext context)
        {
            var recording = (IRecording)context.Target;
            recording.Recording.Add($"{nameof(OnExit)}-{Seed}");
        }
    }
}
