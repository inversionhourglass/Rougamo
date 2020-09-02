using Rougamo.Context;
using System.Drawing;

namespace Rougamo.ILTest
{
    class TheMoAttribute : MoAttribute
    {
        public TheMoAttribute(AccessFlags flags, int intValue, double[] doubleArray)
        {
            Flags = flags;
            IntValue = intValue;
            DoubleArray = doubleArray;
        }

        public override AccessFlags Flags { get; }

        public int IntValue { get; set; }

        public string StringValue { get; set; }

        public double[] DoubleArray { get; set; }

        public int[] IntArray { get; set; }

        public object ObjectValue { get; set; }

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
