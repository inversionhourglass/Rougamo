using Rougamo;
using Rougamo.Context;
using System;
using System.Collections.Generic;

namespace ExUsage.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OrderingAttribute : ExMoAttribute
    {
        private readonly string _flag;

        private List<string> _executions;

        public OrderingAttribute(string flag)
        {
            _flag = flag;
        }

        protected override void ExOnEntry(MethodContext context)
        {
            _executions = (List<string>)context.Arguments[0];
            _executions.Add(GetName(_flag, nameof(ExOnEntry)));
        }

        protected override void ExOnException(MethodContext context)
        {
            _executions.Add(GetName(_flag, nameof(ExOnException)));
        }

        protected override void ExOnSuccess(MethodContext context)
        {
            _executions.Add(GetName(_flag, nameof(ExOnSuccess)));
        }

        protected override void ExOnExit(MethodContext context)
        {
            _executions.Add(GetName(_flag, nameof(ExOnExit)));
        }
        public static List<string> OrderedSucceedExecutions(bool reverse, params string[] flags)
        {
            var list = new List<string>();

            ForEach(reverse, nameof(ExOnEntry), flags, list);
            ForEach(!reverse, nameof(ExOnSuccess), flags, list);
            ForEach(!reverse, nameof(ExOnExit), flags, list);

            return list;
        }

        public static List<string> OrderedFailedExecutions(bool reverse, params string[] flags)
        {
            var list = new List<string>();

            ForEach(reverse, nameof(ExOnEntry), flags, list);
            ForEach(!reverse, nameof(ExOnException), flags, list);
            ForEach(!reverse, nameof(ExOnExit), flags, list);

            return list;
        }

        private static void ForEach(bool reverse, string method, string[] flags, List<string> list)
        {
            if (reverse)
            {
                for (var i = flags.Length - 1; i >= 0; i--)
                {
                    list.Add(GetName(flags[i], method));
                }
            }
            else
            {
                for (int i = 0; i < flags.Length; i++)
                {
                    list.Add(GetName(flags[i], method));
                }
            }
        }

        private static string GetName(string flag, string method) => $"{method}-{flag}";
    }
}
