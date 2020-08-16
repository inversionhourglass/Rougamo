using Rougamo.ImplAssembly;

namespace Rougamo.UsingAssembly
{
    [IgnoreMo]
    public class TestClassIgnore : IRougamo<PublicMo>
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
