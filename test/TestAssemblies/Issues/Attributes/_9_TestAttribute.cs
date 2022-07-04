using Rougamo;
using Rougamo.Context;
using System.Collections.Generic;
using System.Threading;

namespace Issues.Attributes
{
    /// <summary>
    /// #9
    /// </summary>
    public class _9_TestAttribute : MoAttribute
    {
        static AsyncLocal<Issue9.Node> Local = new();

        public override void OnEntry(MethodContext context)
        {
            var parent = Local.Value;
            var value = parent == null ? 0 : parent.Value;
            var node = new Issue9.Node(parent, value + 1);
            Local.Value = node;

            Issue9.Nodestrings.Add(node.ToString());
        }

        public override void OnExit(MethodContext context)
        {
            Local.Value = Local.Value?.Parent;
        }
    }
}
