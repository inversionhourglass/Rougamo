using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.ImplAssembly
{
    public class Mo1Attribute : MoAttribute
    {
    }
    public class Mo2Attribute : MoAttribute
    {
    }
    public class Mo3Attribute : MoAttribute
    {
    }
    public class Mo4Attribute : MoAttribute
    {
    }
    public class Mo5Attribute : MoAttribute
    {
    }

    public class TestRepulsion : MoRepulsion
    {
        public override Type[] Repulsions => new[] { typeof(Mo2Attribute), typeof(Mo3Attribute) };
    }
}
