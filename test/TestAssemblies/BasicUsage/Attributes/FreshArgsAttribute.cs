using Rougamo;
using Rougamo.Context;
using System;

namespace BasicUsage.Attributes
{
    public class FreshArgsAttribute : MoAttribute
    {
        private object[] _args;

        public override void OnEntry(MethodContext context)
        {
            _args = context.Arguments;
        }

        public override void OnException(MethodContext context)
        {
            ArgsCheck(nameof(OnException), context);
        }

        public override void OnSuccess(MethodContext context)
        {
            ArgsCheck(nameof(OnSuccess), context);
        }

        public override void OnExit(MethodContext context)
        {
            ArgsCheck(nameof(OnExit), context);
        }

        private void ArgsCheck(string position, MethodContext context)
        {
            for (var i = 0; i < context.Arguments.Length; i++)
            {
                if (_args[i] != context.Arguments[i])
                {
                    throw new ArgumentException($"Argument [{i}] in {position} not equals with OnEntry");
                }
            }
        }
    }

    public class NonFreshArgsAttribute : FreshArgsAttribute
    {
        public override Feature Features => (Feature.All ^ Feature.FreshArgs) & Feature.All;
    }
}
