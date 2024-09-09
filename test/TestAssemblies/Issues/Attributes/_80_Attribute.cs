using Rougamo;

namespace Issues.Attributes
{
    public class _80_Attribute : MoAttribute
    {
        public override AccessFlags Flags => AccessFlags.Method | AccessFlags.Public | AccessFlags.NonPublic;
    }
}
