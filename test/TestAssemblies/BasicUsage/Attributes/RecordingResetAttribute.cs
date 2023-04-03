using Rougamo;
using Rougamo.Context;

namespace BasicUsage.Attributes
{
    public class RecordingResetAttribute : MoAttribute
    {
        public override double Order => double.MinValue;

        public override void OnEntry(MethodContext context)
        {
            if (context.Method.Name.Contains("Recording")) return;

            var recording = (IRecording)context.Target;
            recording.Recording.Clear();
        }
    }
}
