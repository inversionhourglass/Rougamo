using Rougamo.Context;
using System;
using System.Diagnostics;
using System.Text.Json;

namespace Rougamo.NugetImplAssembly
{
    public class StaticMoAttribute : MoAttribute
    {
        public override AccessFlags Flags { get; } = AccessFlags.Static;

        public override void OnEntry(MethodContext context)
        {
            Console.WriteLine($@"[{this.GetType().FullName}.{nameof(OnEntry)}] context=>
Target: {context.Target},
TargetType: {context.TargetType},
Arguments: {JsonSerializer.Serialize(context.Arguments)}");
        }

        public override void OnException(MethodContext context)
        {
            Console.WriteLine($"[{this.GetType().FullName}.{nameof(OnException)}] {context.Exception}");
        }

        public override void OnExit(MethodContext context)
        {
            Console.WriteLine($"[{this.GetType().FullName}.{nameof(OnExit)}]");
        }

        public override void OnSuccess(MethodContext context)
        {
            Console.WriteLine($"[{this.GetType().FullName}.{nameof(OnSuccess)}] {context.HasReturnValue}<->{JsonSerializer.Serialize(context.ReturnValue)}");
        }
    }
}
