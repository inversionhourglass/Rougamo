using Rougamo;
using Rougamo.Context;
using System.Collections.Generic;

namespace BasicUsage.Attributes
{
    public abstract class AccessableAttribute : MoAttribute
    {
        public override Feature Features => Feature.OnEntry;

        public override void OnEntry(MethodContext context)
        {
            List<string> recording;
            if (context.Target != null)
            {
                recording = ((IRecording)context.Target).Recording;
            }
            else
            {
                recording = (List<string>)context.TargetType.GetProperty("StaticRecording").GetValue(null);
            }
            recording.Add(GetType().Name);
        }
    }

    public class MethodOnlyAttribute : AccessableAttribute
    {
        public override AccessFlags Flags => AccessFlags.Method | AccessFlags.All;
    }

    public class PropertyOnlyAttribute : AccessableAttribute
    {
        public override AccessFlags Flags => AccessFlags.Property | AccessFlags.All;
    }

    public class MethodSetterAttribute : AccessableAttribute
    {
        public override AccessFlags Flags => AccessFlags.Method | AccessFlags.PropertySetter | AccessFlags.All;
    }

    public class MethodGetterAttribute : AccessableAttribute
    {
        public override AccessFlags Flags => AccessFlags.Method | AccessFlags.PropertyGetter | AccessFlags.All;
    }

    public class MethodPropertyAttribute : AccessableAttribute
    {
        public override AccessFlags Flags => AccessFlags.Method | AccessFlags.Property | AccessFlags.All;
    }
}
