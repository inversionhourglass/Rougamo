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
    }
}
