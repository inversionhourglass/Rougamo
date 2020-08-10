using Mono.Cecil;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    internal sealed class RouType
    {
        public RouType(TypeDefinition typeDefinition)
        {
            TypeDef = typeDefinition;
        }

        public TypeDefinition TypeDef { get; set; }

        public List<RouMethod> Methods { get; set; }

        public List<Mo> AssemblyMos { get; set; }

        public List<Mo> TypeMos { get; set; }
    }
}
