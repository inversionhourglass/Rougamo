using Rougamo;
using Rougamo.Context;
using System;
using System.Reflection;

namespace BasicUsage.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class PrefixMatchAttribute : MoAttribute, IMethodDiscoverer
    {
        public string Prefix { get; set; }

        public override Feature Features => Feature.OnEntry;

        public bool IsMatch(MethodInfo method)
        {
            if (string.IsNullOrEmpty(Prefix)) return false;

            return method.Name.StartsWith(Prefix);
        }

        public override void OnEntry(MethodContext context)
        {
            ((IRecording)context.Target).Recording.Add(Order.ToString());
        }
    }
}
