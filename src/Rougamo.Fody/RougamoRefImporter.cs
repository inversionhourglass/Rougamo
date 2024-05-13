using Fody;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    public class RougamoRefImporter(BaseModuleWeaver moduleWeaver)
    {
        private const string SCOPE_ROUGAMO = "Rougamo.dll";

        public TypeReference Import(TypeReference typeRef)
        {
            if (typeRef.Scope.Name == SCOPE_ROUGAMO) return typeRef.ImportInto(moduleWeaver.ModuleDefinition);

            if (typeRef.IsByReference) return new ByReferenceType(Import(typeRef.GetElementType()));
            if (typeRef.IsArray) return new ArrayType(Import(typeRef.GetElementType()));
            if (typeRef is GenericParameter) return typeRef;

            var typeDef = typeRef.Resolve();
            if (moduleWeaver.TryFindTypeDefinition(typeDef.FullName, out var td))
            {
                typeDef = td;
            }

            var iTypeRef = typeDef.ImportInto(moduleWeaver.ModuleDefinition);
            if (typeRef is GenericInstanceType git)
            {
                var igas = new List<TypeReference>(git.GenericArguments.Count);
                foreach (var ga in git.GenericArguments)
                {
                    var iga = Import(ga);
                    igas.Add(iga);
                }
                iTypeRef = iTypeRef.MakeGenericInstanceType(igas.ToArray());
            }

            return iTypeRef;
        }

        public MethodReference Import(MethodReference methodRef)
        {
            var declaringTypeRef = Import(methodRef.DeclaringType);
            var returnTypeRef = Import(methodRef.ReturnType);

            var reference = new MethodReference(methodRef.Name, returnTypeRef, declaringTypeRef)
            {
                HasThis = methodRef.HasThis,
                ExplicitThis = methodRef.ExplicitThis,
                CallingConvention = methodRef.CallingConvention
            };

            foreach (var gp in methodRef.GenericParameters)
            {
                reference.GenericParameters.Add(new GenericParameter(gp.Name, reference));
            }

            foreach (var p in methodRef.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, Import(p.ParameterType)));
            }

            return reference.ImportInto(moduleWeaver.ModuleDefinition);
        }
    }
}
