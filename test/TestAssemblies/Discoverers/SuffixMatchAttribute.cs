using Rougamo;
using Rougamo.Context;
using System.Collections.Generic;
using System.Reflection;

namespace Discoverers
{
    public class SuffixMatchAttribute : MoAttribute, IMethodDiscoverer
    {
        public SuffixMatchAttribute(string suffix)
        {
            Suffix = suffix;
        }

        public string Suffix { get; }

        public override Feature Features => Feature.OnEntry;

        public bool IsMatch(MethodInfo method)
        {
            return method.Name.EndsWith(Suffix);
        }

        public override void OnEntry(MethodContext context)
        {
            ((List<string>)context.TargetType.GetProperty("Recording").GetValue(context.Target)).Add(Order.ToString());
        }
    }
}
