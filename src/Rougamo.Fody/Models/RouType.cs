using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    internal sealed class RouType
    {
        public RouType(TypeDefinition typeDefinition)
        {
            TypeDef = typeDefinition;
            Methods = new List<RouMethod>();
        }

        public TypeDefinition TypeDef { get; }

        public List<RouMethod> Methods { get; }

        public bool HasMo => Methods.Any(rm => rm.Mos.Any());
    }
}
