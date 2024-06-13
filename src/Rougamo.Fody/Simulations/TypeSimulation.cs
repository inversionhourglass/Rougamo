using Mono.Cecil;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System;

namespace Rougamo.Fody.Simulations
{
    /**
     * Prefix naming rules:
     * 
     *  M: Method
     *  F: Field
     * PG: Property getter
     * PS: Property setter
     */
    internal abstract class TypeSimulation : Simulation
    {
        private readonly Dictionary<string, FieldSimulation> _fieldSimulations = [];
        private readonly Dictionary<string, MethodSimulation> _methodSimulations = [];

        protected TypeSimulation(TypeReference typeRef, Dictionary<string, GenericParameter>? genericMap, ModuleDefinition moduleDef) : base(moduleDef)
        {
            if (typeRef is GenericInstanceType && genericMap != null)
            {
                typeRef.GetElementType().ImportInto(moduleDef);
                Ref = typeRef.ReplaceGenericArgs(genericMap);
            }
            else
            {
                Ref = typeRef.ImportInto(moduleDef);
            }
            Def = typeRef.Resolve();
        }

        protected TypeSimulation(TypeReference typeRef, TypeReference[]? generics, ModuleDefinition moduleDef) : base(moduleDef)
        {
            if (typeRef is GenericInstanceType git && generics != null)
            {
                if (git.GenericArguments.Count != generics.Length) throw new RougamoException($"Cannot set generic parameters for {typeRef}, given generic parameters [{string.Join(",", generics.Select(x => x.ToString()))}]");

                git = new GenericInstanceType(typeRef.GetElementType().ImportInto(moduleDef));
                git.GenericArguments.Add(generics);
                Ref = git;
            }
            else
            {
                Ref = typeRef.ImportInto(moduleDef);
            }
            Def = typeRef.Resolve();
        }

        public TypeReference Ref { get; }

        public TypeDefinition Def { get; }

        protected T MethodSimulate<T>(string id, string methodName) where T : MethodSimulation => MethodSimulate<T>(id, x => x.Name == methodName);

        protected T MethodSimulate<T>(string id, Func<MethodDefinition, bool> predicate) where T : MethodSimulation
        {
            if (!_methodSimulations.TryGetValue(id, out var simulation))
            {
                simulation = Def.Methods.Single(predicate).Simulate<T>(this);
                _methodSimulations[id] = simulation;
            }
            return (T)simulation;
        }

        protected T FieldSimulate<T>(string id, string methodName) where T : FieldSimulation => FieldSimulate<T>(id, x => x.Name == methodName);

        protected T FieldSimulate<T>(string id, Func<FieldDefinition, bool> predicate) where T : FieldSimulation
        {
            if (!_fieldSimulations.TryGetValue(id, out var simulation))
            {
                simulation = Def.Fields.Single(predicate).Simulate<T>(this);
                _fieldSimulations[id] = simulation;
            }
            return (T)simulation;
        }

        public static implicit operator TypeReference(TypeSimulation value) => value.Ref;
    }

    internal static class TypeSimulationExtensions
    {
        private static readonly Dictionary<Type, Func<TypeReference, Dictionary<string, GenericParameter>?, ModuleDefinition, object>> _CacheMap = [];
        private static readonly Dictionary<Type, Func<TypeReference, TypeReference[]?, ModuleDefinition, object>> _CacheArray = [];

        public static T Simulate<T>(this TypeReference typeRef, ModuleDefinition moduleDef, Dictionary<string, GenericParameter>? genericMap = null) where T : TypeSimulation
        {
            var type = typeof(T);

            if (!_CacheMap.TryGetValue(type, out var ctor))
            {
                var ctorInfo = type.GetConstructor([typeof(TypeReference), typeof(Dictionary<string, GenericParameter>), typeof(ModuleDefinition)]);
                var psExp = ctorInfo.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name));
                var ctorExp = Expression.New(ctorInfo, psExp);
                ctor = Expression.Lambda<Func<TypeReference, Dictionary<string, GenericParameter>?, ModuleDefinition, object>>(ctorExp, psExp).Compile();

                _CacheMap[type] = ctor;
            }

            return (T)ctor(typeRef, genericMap, moduleDef);
        }

        public static T Simulate<T>(this TypeReference typeRef, ModuleDefinition moduleDef, TypeReference[] generics) where T : TypeSimulation
        {
            var type = typeof(T);

            if (!_CacheArray.TryGetValue(type, out var ctor))
            {
                var ctorInfo = type.GetConstructor([typeof(TypeReference), typeof(TypeReference[]), typeof(ModuleDefinition)]);
                var psExp = ctorInfo.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name));
                var ctorExp = Expression.New(ctorInfo, psExp);
                ctor = Expression.Lambda<Func<TypeReference, TypeReference[]?, ModuleDefinition, object>>(ctorExp, psExp).Compile();

                _CacheArray[type] = ctor;
            }

            return (T)ctor(typeRef, generics, moduleDef);
        }
    }
}
