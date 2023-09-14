using Rougamo;
using Rougamo.Context;
using System.Collections.Generic;

namespace PatternUsage.Attributes
{
    public abstract class SetOnEntryAttribute : MoAttribute
    {
        public override Feature Features => Feature.OnEntry;

        public virtual string Name => GetType().Name;

        public override void OnEntry(MethodContext context)
        {
            if (context.Arguments.Length == 0) return;
            var executed = context.Arguments[0] as List<string>;
            if (executed == null) return;
            executed.Add(Name);
        }
    }
}
