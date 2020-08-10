using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.ImplAssembly
{
    public class MultiRepulsion : MoRepulsion
    {
        public override Type[] Repulsions => new []{ typeof(object), typeof(MoRepulsion) };
    }

    public class MultiRepulsion1 : MoRepulsion
    {
        public override Type[] Repulsions { get; } = new[] { typeof(MultiRepulsion), typeof(int) };
    }

    public class MultiRepulsion2 : MoRepulsion
    {
        public MultiRepulsion2()
        {
            Repulsions = new[]
            {
                typeof(Guid),
                typeof(IMo)
            };
        }

        public override Type[] Repulsions { get; }
    }
}
