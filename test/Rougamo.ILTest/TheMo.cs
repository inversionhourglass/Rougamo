using Rougamo.Context;

namespace Rougamo.ILTest
{
    class TheMo : IMo
    {
        public AccessFlags Flags => AccessFlags.All;

        public void OnEntry(EntryContext context)
        {
        }

        public void OnException(ExceptionContext context)
        {
        }

        public void OnExit(ExitContext context)
        {
        }

        public void OnSuccess(SuccessContext context)
        {
        }
    }
}
