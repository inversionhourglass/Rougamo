using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    internal static class TypeReferenceExtensions
    {
        public static TypeReference MakeReference(this TypeDefinition typeDef)
        {
            if (!typeDef.HasGenericParameters) return typeDef;

            var generic = new GenericInstanceType(typeDef);
            foreach (var genericParameter in typeDef.GenericParameters)
            {
                generic.GenericArguments.Add(genericParameter);
            }

            return generic;
        }

        public static List<GenericInstanceType> GetGenericInterfaces(this TypeDefinition typeDef, string interfaceName, int genericCount)
        {
            var interfaces = new List<GenericInstanceType>();
            do
            {
                var titf = typeDef.Interfaces.Select(itf => itf.InterfaceType).Where(itfRef => itfRef.IsGeneric(interfaceName, genericCount));
                interfaces.AddRange(titf.Cast<GenericInstanceType>());
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                typeDef = typeDef.BaseType?.Resolve();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            } while (typeDef != null);

            return interfaces;
        }

        public static TypeDefinition? GetInterfaceDefinition(this TypeDefinition typeDef, string interfaceName)
        {
            do
            {
                var interfaceRef = typeDef.Interfaces.Select(itf => itf.InterfaceType).Where(itfRef => itfRef.Is(interfaceName));
                if (interfaceRef.Any()) return interfaceRef.First().Resolve();
                typeDef = typeDef.BaseType.Resolve();
            } while (typeDef != null);

            return null;
        }

        #region Field
        public static TypeDefinition AddUniqueField(this TypeDefinition typeDef, FieldDefinition? fieldDef)
        {
            if (fieldDef == null) return typeDef;

            var fields = typeDef.Fields;
            var currentFieldDef = fields.FirstOrDefault(x => x.Name == fieldDef.Name);
            if (currentFieldDef != null) fields.Remove(currentFieldDef);
            fields.Add(fieldDef);

            return typeDef;
        }
        #endregion Field

        #region Property
        public static MethodDefinition GetPropertySetterDef(this TypeDefinition typeDef, string propertyName)
        {
            var propertyDef = typeDef.Properties.FirstOrDefault(pd => pd.Name == propertyName);
            if (propertyDef != null) return propertyDef.SetMethod ?? throw new RougamoException($"{typeDef.FullName} has property({propertyName}) but this property does not contain set method.");

            var baseTypeDef = typeDef.BaseType.Resolve();
            if (baseTypeDef.FullName == typeof(object).FullName)
                throw new RougamoException($"can not find property({propertyName}) from {typeDef.FullName}");
            return GetPropertySetterDef(baseTypeDef, propertyName);
        }

        public static MethodDefinition GetPropertyGetterDef(this TypeDefinition typeDef, string propertyName)
        {
            var propertyDef = typeDef.Properties.FirstOrDefault(pd => pd.Name == propertyName);
            if (propertyDef != null) return propertyDef.GetMethod ?? throw new RougamoException($"{typeDef.FullName} has property({propertyName}) but this property does not contain get method.");

            var baseTypeDef = typeDef.BaseType.Resolve();
            if (baseTypeDef.FullName == typeof(object).FullName)
                throw new RougamoException($"can not find property({propertyName}) from {typeDef.FullName}");
            return GetPropertyGetterDef(baseTypeDef, propertyName);
        }
        #endregion Property

        #region Method
        public static MethodDefinition GetMethod(this TypeReference typeRef, string name, bool recursion) => typeRef.GetMethod(recursion, md => md.Name == name) ?? throw new RougamoException($"Cannot find the method {name} from {typeRef}");

        public static MethodDefinition? GetMethod(this TypeReference typeRef, bool recursion, Func<MethodDefinition, bool> predicate)
        {
            if (typeRef is not TypeDefinition typeDef) typeDef = typeRef.Resolve();

            MethodDefinition? methodDef = null;
            if (typeDef.IsInterface)
            {
                methodDef = typeDef.Methods.SingleOrDefault(predicate);
                if (methodDef == null)
                {
                    foreach (var @interface in typeDef.Interfaces)
                    {
                        methodDef = @interface.InterfaceType.Resolve().Methods.SingleOrDefault(predicate);
                        if (methodDef != null) break;
                    }
                }

                return methodDef;
            }

            do
            {
                methodDef = typeDef.Methods.SingleOrDefault(predicate);
            } while (methodDef == null && recursion && (typeDef = typeDef.BaseType.Resolve()).FullName != typeof(object).FullName);

            return methodDef;
        }
        #endregion Method

        #region Generic
        public static int GetGenericParameterIndexByName(this TypeDefinition typeDef, string name)
        {
            for (var i = 0; i < typeDef.GenericParameters.Count; i++)
            {
                if (typeDef.GenericParameters[i].Name == name) return i;
            }

            throw new RougamoException($"Cannot find the generic parameter named {name} in the type {typeDef}");
        }

        public static GenericParameter[] GetSelfGenerics(this TypeDefinition typeDef)
        {
            var declaringTypeDef = typeDef.DeclaringType;
            if (declaringTypeDef == null) return typeDef.GenericParameters.ToArray();

            var declaringTypeGenericNames = declaringTypeDef.GenericParameters.Select(x => x.Name).ToArray();
            return typeDef.GenericParameters.Where(x => !declaringTypeGenericNames.Contains(x.Name)).ToArray();
        }

        public static Dictionary<string, TypeReference> GetGenericMap(this TypeReference typeRef)
        {
            if (typeRef is GenericParameter gp) return new() { { gp.Name, gp } };
            if (typeRef is TypeDefinition td) return td.GenericParameters.ToDictionary(x => x.Name, x => (TypeReference)x);
            if (typeRef is not GenericInstanceType git) return [];

            var typeDef = typeRef.Resolve();
            var map = new Dictionary<string, TypeReference>();

            for (var i = 0; i < typeDef.GenericParameters.Count; i++)
            {
                map[typeDef.GenericParameters[i].Name] = git.GenericArguments[i];
            }

            return map;
        }

        public static Dictionary<string, TypeReference> GetGenericMap(this MethodDefinition methodDef)
        {
            var map = methodDef.DeclaringType.GetGenericMap();

            foreach (var gp in methodDef.GenericParameters)
            {
                map[gp.Name] = gp;
            }

            return map;
        }

        public static TypeReference ReplaceGenericArgs(this TypeReference typeRef, Dictionary<string, TypeReference>? genericMap)
        {
            if (genericMap == null) return typeRef;

            if (typeRef is GenericParameter gp && genericMap.TryGetValue(gp.Name, out var value)) return value;

            if (typeRef is ArrayType at) return new ArrayType(ReplaceGenericArgs(at.ElementType, genericMap));

            if (typeRef is GenericInstanceType git)
            {
                var replacedGit = new GenericInstanceType(git.GetElementType());
                foreach (var generic in git.GenericArguments)
                {
                    replacedGit.GenericArguments.Add(generic.ReplaceGenericArgs(genericMap));
                }

                return replacedGit;
            }

            if (typeRef is TypeDefinition typeDef && typeDef.HasGenericParameters)
            {
                var replacedGit = new GenericInstanceType(typeDef.GetElementType());
                foreach (var generic in typeDef.GenericParameters)
                {
                    replacedGit.GenericArguments.Add(generic.ReplaceGenericArgs(genericMap));
                }

                return replacedGit;
            }

            return typeRef;
        }
        #endregion Generic

        #region Infering Import
        public static TypeReference Import(this TypeReference typeRef, params TypeReference[] inferenceChain)
        {
            var chainLast = inferenceChain.Last();
            TypeReference[] genericArguments;
            if (chainLast is GenericInstanceType importingTypeRef)
            {
                genericArguments = importingTypeRef.GenericArguments.ToArray();
            }
            else if (chainLast is GenericParameter)
            {
                importingTypeRef = null!;
                genericArguments = [chainLast];
            }
            else
            {
                return chainLast.ImportInto(typeRef.Module);
            }

            for (var i = inferenceChain.Length - 2; i >= 0; i--)
            {
                if (inferenceChain[i] is not GenericInstanceType preTypeRef) throw new RougamoException($"Item in inference chain must be GenericInstanceType, but {inferenceChain[i]} is not. Cannot import {importingTypeRef} into {typeRef}");

                MappingGenerics(genericArguments, preTypeRef);
            }

            MappingGenerics(genericArguments, typeRef);

            if (importingTypeRef == null) return genericArguments[0].ImportInto(typeRef.Module);

            var git = new GenericInstanceType(importingTypeRef.ElementType.ImportInto(typeRef.Module));
            git.GenericArguments.Add(genericArguments);
            return git;
        }

        private static void MappingGenerics(TypeReference[] generics, TypeReference typeRef)
        {
            if (typeRef is GenericInstanceType git)
            {
                MappingGenerics(generics, git);
            }
            else if (typeRef is TypeDefinition typeDef)
            {
                MappingGenerics(generics, typeDef);
            }
        }

        private static void MappingGenerics(TypeReference[] generics, GenericInstanceType git)
        {
            var typeDef = git.Resolve();
            for (var j = 0; j < generics.Length; j++)
            {
                var generic = generics[j];
                if (generic is GenericParameter gp)
                {
                    var gpIndex = typeDef.GetGenericParameterIndexByName(gp.Name);
                    generics[j] = git.GenericArguments[gpIndex];
                }
                else if (generic is GenericInstanceType gitt)
                {
                    var innerGenerics = gitt.GenericArguments.ToArray();
                    MappingGenerics(innerGenerics, git);
                    var newGit = new GenericInstanceType(gitt.ElementType);
                    newGit.GenericArguments.Add(innerGenerics);
                    generics[j] = newGit;
                }
                else
                {
                    generics[j] = generic;
                }
            }
        }

        private static void MappingGenerics(TypeReference[] generics, TypeDefinition typeDef)
        {
            for (var j = 0; j < generics.Length; j++)
            {
                var generic = generics[j];
                if (generic is GenericParameter gp)
                {
                    var gpIndex = typeDef.GetGenericParameterIndexByName(gp.Name);
                    generics[j] = typeDef.GenericParameters[gpIndex];
                }
                else if (generic is GenericInstanceType git)
                {
                    var innerGenerics = git.GenericArguments.ToArray();
                    MappingGenerics(innerGenerics, typeDef);
                    var newGit = new GenericInstanceType(git.ElementType);
                    newGit.GenericArguments.Add(innerGenerics);
                    generics[j] = newGit;
                }
                else
                {
                    generics[j] = generic;
                }
            }
        }
        #endregion Infering Import
    }
}
