using Rougamo;
using System;
using System.Linq;
using System.Reflection;

namespace BasicUsage.Attributes
{
    internal class MethodDiscoveryAttribute : MoAttribute
    {
        public override Type DiscovererType { get; set; } = typeof(MethodDiscoverer);
    }

    public class MethodDiscoverer : IMethodDiscoverer
    {
        public bool IsMatch(MethodInfo method)
        {
            if (method.DeclaringType == null) return true;
            return method.DeclaringType.GetProperties().All(x => x.GetMethod != method && x.SetMethod != method);
        }
    }
}
