using Rougamo.ImplAssembly;

namespace Rougamo.UsingAssembly
{
    [IgnoreMo(MoTypes = new[] { typeof(StaticMoAttribute) })]
    public class TestSelectionIgnore
    {
        public static void Static()
        {

        }

        public void Instance()
        {

        }

        private static void PrivateStatic()
        {

        }

        private void PrivateInstance()
        {

        }
    }
}
