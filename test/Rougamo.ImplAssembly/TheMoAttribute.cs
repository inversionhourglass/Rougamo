using Rougamo.Context;

namespace Rougamo.ImplAssembly
{
    public class TheMoAttribute : MoAttribute
    {
        public string Name { get; set; }

        public override void OnEntry(EntryContext context)
        {
        }

        public override void OnException(ExceptionContext context)
        {
        }

        public override void OnExit(ExitContext context)
        {
        }

        public override void OnSuccess(SuccessContext context)
        {
        }
    }
}
