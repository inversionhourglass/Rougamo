using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.ImplAssembly
{
    public class AccessableMoAttribute : OneMoAttribute
    {
        public override AccessFlags Flags { get; } = AccessFlags.Instance;
    }
}
