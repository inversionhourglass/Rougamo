using Rougamo.ImplAssembly;
using System;

namespace Rougamo.UsingAssembly
{
    class AllMoRepulsions : MoRepulsion
    {
        public override Type[] Repulsions
        {
            get
            {
                return new[] {
                    typeof(InstanceMoAttribute), typeof(StaticMoAttribute),
                    typeof(PublicMoAttribute), typeof(StaticMoAttribute),
                    typeof(PublicMo), typeof(PublicInstanceMo)
                };
            }
        }
    }
}
