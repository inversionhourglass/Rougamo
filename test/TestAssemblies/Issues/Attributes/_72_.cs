using Rougamo;
using Rougamo.Context;
using System.Collections.Generic;

namespace Issues.Attributes
{
    public struct _72_ : IMo
    {
        public _72_() { }

        public AccessFlags Flags { get; } = AccessFlags.All;

        public string Pattern { get; } = null;

        public Feature Features { get; } = Feature.OnEntry;

        public double Order { get; } = 1;

        public Omit MethodContextOmits { get; } = Omit.None;

        public void OnEntry(MethodContext context)
        {
            var list = (List<string>)context.Arguments[0];
            list.Add(nameof(OnEntry));
        }

        public void OnException(MethodContext context)
        {
            var list = (List<string>)context.Arguments[0];
            list.Add(nameof(OnException));
        }

        public void OnExit(MethodContext context)
        {
            var list = (List<string>)context.Arguments[0];
            list.Add(nameof(OnExit));
        }

        public void OnSuccess(MethodContext context)
        {
            var list = (List<string>)context.Arguments[0];
            list.Add(nameof(OnSuccess));
        }
    }
}
