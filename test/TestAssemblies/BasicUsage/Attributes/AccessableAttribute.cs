using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;
using System.Collections.Generic;

namespace BasicUsage.Attributes
{
    [Advice(Feature.OnEntry)]
    public abstract class AccessableAttribute : MoAttribute
    {
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

    [Pointcut(AccessFlags.Method | AccessFlags.All)]
    public class MethodOnlyAttribute : AccessableAttribute
    {
    }

    [Pointcut(AccessFlags.Property | AccessFlags.All)]
    public class PropertyOnlyAttribute : AccessableAttribute
    {
    }

    [Pointcut(AccessFlags.Method | AccessFlags.PropertySetter | AccessFlags.All)]
    public class MethodSetterAttribute : AccessableAttribute
    {
    }

    [Pointcut(AccessFlags.Method | AccessFlags.PropertyGetter | AccessFlags.All)]
    public class MethodGetterAttribute : AccessableAttribute
    {
    }

    [Pointcut(AccessFlags.Method | AccessFlags.Property | AccessFlags.All)]
    public class MethodPropertyAttribute : AccessableAttribute
    {
    }
}
