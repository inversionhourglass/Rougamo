using Rougamo;
using Rougamo.Context;

namespace Issues.Attributes
{
    public class _40_Attribute : MoAttribute
    {
        public static int ReplacedValue = -1;

        public new AccessFlags Flags { get; set; } = AccessFlags.NonPublic;

        public override void OnEntry(MethodContext context)
        {
            context.ReplaceReturnValue(this, ReplacedValue);
        }
    }
}
