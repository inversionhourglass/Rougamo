using Rougamo.Context;
using System;
using System.Diagnostics;
using System.Text.Json;

namespace Rougamo.ImplAssembly
{
    public class StaticMoAttribute : MoAttribute
    {
        public override AccessFlags Flags { get; } = AccessFlags.Static;

        public override void OnEntry(MethodContext context)
        {
            Debug.Print($@"[{this.GetType().FullName}.{nameof(OnEntry)}] context=>
Target: {context.Target},
TargetType: {context.TargetType},
Arguments: {JsonSerializer.Serialize(context.Arguments)}");
        }

        public override void OnException(MethodContext context)
        {
            Debug.Print($"[{this.GetType().FullName}.{nameof(OnException)}] {context.Exception}");
        }

        public override void OnExit(MethodContext context)
        {
            Debug.Print($"[{this.GetType().FullName}.{nameof(OnExit)}]");
        }

        public override void OnSuccess(MethodContext context)
        {
            Debug.Print($"[{this.GetType().FullName}.{nameof(OnSuccess)}] {JsonSerializer.Serialize(context.ReturnValue)}");
        }
    }
}
