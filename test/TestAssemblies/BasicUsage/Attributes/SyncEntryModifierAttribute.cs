using Rougamo;
using Rougamo.Context;
using System.Collections.Generic;

namespace BasicUsage.Attributes
{
    public class SyncEntryModifierAttribute : MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            var datas = (List<string>)context.Arguments[0];
            datas.Add(nameof(OnEntry));
            if (context.RealReturnType == typeof(string))
            {
                context.ReplaceReturnValue(this, nameof(OnEntry));
            }
            else if(context.RealReturnType == typeof(void))
            {
                context.ReplaceReturnValue(this, null);
            }
        }

        public override void OnException(MethodContext context)
        {
            var datas = (List<string>)context.Arguments[0];
            datas.Add(nameof(OnException));
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
