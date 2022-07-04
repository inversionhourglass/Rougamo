using Rougamo.Context;
using System;

namespace Issues.Attributes
{
    /// <summary>
    /// #7, #8
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class _7_8_LogAttribute : Rougamo.MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
        }

        public override void OnException(MethodContext context)
        {
            context.HandledException(this, null);
        }

        public override void OnExit(MethodContext context)
        {
        }

        public override void OnSuccess(MethodContext context)
        {
        }
    }
}
