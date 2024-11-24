using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;
using System;

namespace BasicUsage.Attributes
{
    [Advice(Feature.Observe)]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class FeatureIteratorAttribute : MoAttribute
    {
        public FeatureIteratorAttribute(int seed)
        {
            Seed = seed;
        }

        public int Seed { get; }

        public override void OnEntry(MethodContext context)
        {
            var recording = (IRecording)context.Target;
            recording.Recording.Add($"{nameof(OnEntry)}-{Seed}");

            if(context.Arguments.Length == 1 && context.Arguments[0].GetType() == typeof(Guid))
            {
                context.RewriteArguments = true;
                context.Arguments[0] = Guid.Empty;
            }
        }

        public override void OnException(MethodContext context)
        {
            var recording = (IRecording)context.Target;
            recording.Recording.Add($"{nameof(OnException)}-{Seed}");
        }

        public override void OnSuccess(MethodContext context)
        {
            var recording = (IRecording)context.Target;
            recording.Recording.Add($"{nameof(OnSuccess)}-{Seed}");
        }

        public override void OnExit(MethodContext context)
        {
            var recording = (IRecording)context.Target;
            recording.Recording.Add($"{nameof(OnExit)}-{Seed}");
        }
    }
}
