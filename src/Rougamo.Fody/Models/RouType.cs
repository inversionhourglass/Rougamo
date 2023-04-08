using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    internal sealed class RouType
    {
        public RouType(TypeDefinition typeDefinition)
        {
            TypeDef = typeDefinition;
            Type = TypeDef.ResolveType() ?? throw new RougamoException($"Cannot resolve type {TypeDef.FullName}");
            Methods = new List<RouMethod>();
        }

        public TypeDefinition TypeDef { get; }

        public Type Type { get; }

        public List<RouMethod> Methods { get; }

        public bool HasMo => Methods.Any(rm => rm.MosAny());
    }
}
