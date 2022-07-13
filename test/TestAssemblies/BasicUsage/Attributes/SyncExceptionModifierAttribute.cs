using Rougamo;
using Rougamo.Context;
using System.Collections.Generic;

namespace BasicUsage.Attributes
{
    public class SyncExceptionModifierAttribute : MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            var datas = (List<string>)context.Arguments[0];
            datas.Add(nameof(OnEntry));
        }

        public override void OnException(MethodContext context)
        {
            var datas = (List<string>)context.Arguments[0];
            datas.Add(nameof(OnException));
            context.HandledException(this, nameof(OnException));
        }

        public override void OnSuccess(MethodContext context)
        {
            var datas = (List<string>)context.Arguments[0];
            datas.Add(nameof(OnSuccess));
        }

        public override void OnExit(MethodContext context)
        {
            var datas = (List<string>)context.Arguments[0];
            datas.Add(nameof(OnExit));
        }
    }
}
