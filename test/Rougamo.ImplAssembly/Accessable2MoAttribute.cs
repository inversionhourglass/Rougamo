using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.ImplAssembly
{
    public class Accessable2MoAttribute : AccessableMoAttribute
    {
        public override AccessFlags Flags => AccessFlags.Public;
    }
    public class Accessable3MoAttribute : Accessable2MoAttribute
    {
    }
}
