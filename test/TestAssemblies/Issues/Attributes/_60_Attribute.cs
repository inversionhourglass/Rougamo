using Rougamo;
using Rougamo.Context;
using System.Collections.Generic;

namespace Issues.Attributes
{
    public class _60_Attribute : MoAttribute
    {
        public static readonly string[] SUCCEED = [nameof(OnEntry), nameof(OnSuccess), nameof(OnExit)];
        public static readonly string[] FAILED = [nameof(OnEntry), nameof(OnException), nameof(OnExit)];

        private List<string> _executed;

        public override void OnEntry(MethodContext context)
        {
            if (context.Arguments.Length == 1 && context.Arguments[0] is List<string> executed)
            {
                _executed = executed;

                _executed.Add(nameof(OnEntry));
            }
        }

        public override void OnSuccess(MethodContext context)
        {
            _executed.Add(nameof(OnSuccess));
        }

        public override void OnException(MethodContext context)
        {
            _executed.Add(nameof(OnException));
        }

        public override void OnExit(MethodContext context)
        {
            _executed.Add(nameof(OnExit));
        }
    }
}
