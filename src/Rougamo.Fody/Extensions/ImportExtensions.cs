using Fody;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    internal static class ImportExtensions
    {
        private const string SCOPE_ROUGAMO = "Rougamo.dll";
        private const string SCOPE_VALUE_TUPLE = "System.ValueTuple.dll";

        public static TypeReference Import(this BaseModuleWeaver moduleWeaver, TypeReference typeRef)
        {
            //if (typeRef.Scope.Name == SCOPE_ROUGAMO) return typeRef.ImportInto(moduleWeaver.ModuleDefinition);

            if (typeRef is ByReferenceType brt) return new ByReferenceType(Import(moduleWeaver, brt.ElementType));
            if (typeRef is GenericParameter) return typeRef;
            if (typeRef is ArrayType at)
            {
                var arrayType = new ArrayType(Import(moduleWeaver, at.ElementType), at.Rank);
                for (var i = 0; i < at.Rank; i++)
                {
                    arrayType.Dimensions[i] = at.Dimensions[i];
                }
                return arrayType;
            }

            var typeDef = typeRef.Resolve();
            /**
             * todo: 真棒棒呢，曾经在#31出现过的ValueTuple的问题，当时搞得我只能把肉夹馍用到ValueTuple的地方全都改掉，现在更厉害了，
             * 唯独net461的ValueTuple默认scope是System.ValueTuple.dll(其实不确定net462, net47等是否也是这样，这里framework版本仅测试net461和net48)，
             * 同时ValueTuple还能从基础库mscorlib中获取到，然而如果使用mscorlib中的ValueTuple，在运行时就会报错，现在不细究底层原因了，
             * 有的是其他头疼的问题，先跳过就完了。若要复现问题，删除下面if判断中`typeDef.Scope.Name != SCOPE_VALUE_TUPLE`部分，
             * 然后执行net461下的测试用例`ExecutionTupleSyntaxTest`
             */
            if (typeDef.Scope.Name != SCOPE_VALUE_TUPLE && moduleWeaver.TryFindTypeDefinition(typeDef.FullName, out var td))
            {
                typeDef = td;
            }

            var iTypeRef = typeDef.ImportInto(moduleWeaver.ModuleDefinition);
            if (typeRef is GenericInstanceType git)
            {
                var igas = new List<TypeReference>(git.GenericArguments.Count);
                foreach (var ga in git.GenericArguments)
                {
                    var iga = Import(moduleWeaver, ga);
                    igas.Add(iga);
                }
                iTypeRef = iTypeRef.MakeGenericInstanceType(igas.ToArray());
            }

            return iTypeRef;
        }

        public static MethodReference Import(this BaseModuleWeaver moduleWeaver, MethodReference methodRef)
        {
            var declaringTypeRef = Import(moduleWeaver, methodRef.DeclaringType);
            var returnTypeRef = Import(moduleWeaver, methodRef.ReturnType);

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
                reference.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, Import(moduleWeaver, p.ParameterType)));
            }

            return moduleWeaver.ModuleDefinition.ImportReference(reference);
        }

        public static TypeReference ImportInto(this TypeReference typeRef, ModuleWeaver moduleWeaver)
        {
            return moduleWeaver.Import(typeRef);
        }

        public static MethodReference ImportInto(this MethodReference methodRef, ModuleWeaver moduleWeaver)
        {
            return moduleWeaver.Import(methodRef);
        }

        public static TypeReference ImportInto(this TypeReference typeRef, ModuleDefinition moduleDef)
        {
            return moduleDef.ImportReference(typeRef);
        }

        public static MethodReference ImportInto(this MethodReference methodRef, ModuleDefinition moduleDef)
        {
            return moduleDef.ImportReference(methodRef);
        }
    }
}
