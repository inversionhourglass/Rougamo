using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;
using System;

namespace BasicUsage.Attributes
{
    public class FreshArgsAttribute : MoAttribute
    {
        private object[] _args;

        public override void OnEntry(MethodContext context)
        {
            _args = new object[context.Arguments.Length];
            for (int i = 0; i < context.Arguments.Length; i++)
            {
                _args[i] = context.Arguments[i];
            }
        }

        public override void OnException(MethodContext context)
        {
            context.HandledException(this, new Tuple<object[], object[]>(_args, context.Arguments));
        }

        public override void OnSuccess(MethodContext context)
        {
            context.ReplaceReturnValue(this, new Tuple<object[], object[]>(_args, context.Arguments));
        }
    }

    [Advice((Feature.All ^ Feature.FreshArgs) & Feature.All)]
    public class NonFreshArgsAttribute : FreshArgsAttribute
    {
    }
}
