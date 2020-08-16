using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.UsingAssembly
{
    public class TestRepulsion : TestRepulsionBase
    {
        public static void Static()
        {

        }

        public void Instance()
        {

        }

        [Obsolete]
        private static void PrivateStatic()
        {

        }

        private void PrivateInstance()
        {

        }
    }
}
