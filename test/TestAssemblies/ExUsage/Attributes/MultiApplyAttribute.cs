using Rougamo;
using Rougamo.Context;
using System;

namespace ExUsage.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MultiApplyAttribute : ExMoAttribute
    {
        public const int FirstChangedValue = int.MinValue;
        public const int SecondChangedValue = int.MaxValue;

        public MultiApplyAttribute(object seed) { }

        protected override void ExOnSuccess(MethodContext context)
        {
            if (context.ExReturnValue is int v && v == FirstChangedValue)
            {
                context.ReplaceReturnValue(this, SecondChangedValue);
            }
            else
            {
                context.ReplaceReturnValue(this, FirstChangedValue);
            }
        }

        protected override void ExOnException(MethodContext context)
        {
            if (context.ExceptionHandled)
            {
                context.ReplaceReturnValue(this, SecondChangedValue);
            }
            else
            {
                context.HandledException(this, FirstChangedValue);
            }
        }
    }
}
