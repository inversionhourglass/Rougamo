using Rougamo.ImplAssembly;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.UsingAssembly
{
    public class TestRougamo : IRougamo<PublicInstanceMo>
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

        [Obsolete]
        private void PrivateInstance()
        {

        }
    }
}
