using Mono.Cecil;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    internal sealed class RouMethod
    {
        public MethodDefinition MethodDef { get; set; }

        public List<Mo> MethodMos { get; set; }

        public bool IsAsync { get; set; }
    }
}
