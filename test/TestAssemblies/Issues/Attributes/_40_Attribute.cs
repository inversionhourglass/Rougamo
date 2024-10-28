using Rougamo;
using Rougamo.Context;
using Rougamo.Flexibility;

namespace Issues.Attributes
{
    public class _40_Attribute : MoAttribute, IFlexibleModifierPointcut
    {
        public static int ReplacedValue = -1;

        public AccessFlags Flags { get; set; } = AccessFlags.NonPublic;

        public override void OnEntry(MethodContext context)
        {
            context.ReplaceReturnValue(this, ReplacedValue);
        }
    }
}
