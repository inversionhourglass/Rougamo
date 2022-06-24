using Rougamo.Context;
using System;
using System.IO;

namespace BasicUsage.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LogAttribute : Rougamo.MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            File.AppendAllLines(@"D:\issue8.txt", new[] { $"{context.Method.Name} onentry" });
        }

        public override void OnException(MethodContext context)
        {
            File.AppendAllLines(@"D:\issue8.txt", new[] { $"{context.Method.Name} onexception" });
            context.HandledException(this, null);
        }

        public override void OnExit(MethodContext context)
        {
            File.AppendAllLines(@"D:\issue8.txt", new[] { $"{context.Method.Name} onexit" });
        }

        public override void OnSuccess(MethodContext context)
        {
            File.AppendAllLines(@"D:\issue8.txt", new[] { $"{context.Method.Name} onsuccess" });
        }
    }
}
