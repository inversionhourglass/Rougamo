using Rougamo;
using Rougamo.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicUsage.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ForceSyncAttribute : RawMoAttribute
    {
        private readonly char[] _history = new char[3];

        public new ForceSync ForceSync { get; set; }

        public new double Order { get; set; }

        public override void OnEntry(MethodContext context)
        {
            _history[0] = 'S';
        }

        public override ValueTask OnEntryAsync(MethodContext context)
        {
            _history[0] = 'A';
            return default;
        }

        public override void OnSuccess(MethodContext context)
        {
            _history[1] = 'S';
        }

        public override ValueTask OnSuccessAsync(MethodContext context)
        {
            _history[1] = 'A';
            return default;
        }

        public override void OnException(MethodContext context)
        {
            _history[1] = 'S';
        }

        public override ValueTask OnExceptionAsync(MethodContext context)
        {
            _history[1] = 'A';
            return default;
        }

        public override void OnExit(MethodContext context)
        {
            _history[2] = 'S';
            SetExecution(context);
        }

        public override ValueTask OnExitAsync(MethodContext context)
        {
            _history[2] = 'A';
            SetExecution(context);
            return default;
        }

        private void SetExecution(MethodContext context)
        {
            ((List<string>)context.Arguments[0]).Add(new(_history));
        }
    }
}
