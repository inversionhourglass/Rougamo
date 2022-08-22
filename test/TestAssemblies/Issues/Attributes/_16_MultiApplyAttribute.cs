using Rougamo;
using Rougamo.Context;
using System;
using System.Collections.Generic;

namespace Issues.Attributes
{
    /// <summary>
    /// #16
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class _16_MultiApplyAttribute : MoAttribute
    {
        private readonly string _flag;

        public enum Em
        {
            A,
            B
        }

        public _16_MultiApplyAttribute(string flag, Em em, params string[] objs)
        {
            _flag = flag;
        }

        public override void OnEntry(MethodContext context)
        {
            var list = (List<string>)context.Arguments[0];
            list.Add(_flag);
        }
    }
}
