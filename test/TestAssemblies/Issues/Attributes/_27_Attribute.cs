using Rougamo;
using Rougamo.Context;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Issues.Attributes
{
    public class _27_Attribute : MoAttribute
    {
        public override void OnSuccess(MethodContext context)
        {
            if (typeof(Task).IsAssignableFrom(((MethodInfo)context.Method).ReturnType))
            {
                context.ReplaceReturnValue(this, Task.FromResult(new object()));
            }
            else
            {
                context.ReplaceReturnValue(this, new object());
            }
        }
    }

    public class _27Ex_Attribute : ExMoAttribute
    {
        protected override void ExOnSuccess(MethodContext context)
        {
            Console.WriteLine("exsuccess");
            context.ReplaceReturnValue(this, new object());
        }
    }
}
