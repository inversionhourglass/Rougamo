using Rougamo;
using Rougamo.Context;
using System;
using System.Collections.Generic;

namespace Issues.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class _20_Attribute : MoAttribute
    {
        private readonly string _flag;

        public _20_Attribute(string flag)
        {
            _flag = flag;
        }

        public override void OnEntry(MethodContext context)
        {
            ((List<string>)context.Arguments[0]).Add($"{nameof(OnEntry)}-{_flag}");
        }

        public override void OnExit(MethodContext context)
        {
            ((List<string>)context.Arguments[0]).Add($"{nameof(OnExit)}-{_flag}");
        }
    }
}
