using Rougamo.ImplAssembly;

namespace Rougamo.UsingAssembly
{
    public class TestMethodIgnore : IRougamo<InstanceMoAttribute>
    {
        public static void Static()
        {

        }

        [IgnoreMo]
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
