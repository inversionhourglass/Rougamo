using Mono.Cecil;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System;

namespace Rougamo.Fody.Simulations
{
    internal class TypeSimulation : Simulation
    {
        private readonly Dictionary<string, FieldSimulation> _fieldSimulations = [];
        private readonly Dictionary<string, MethodSimulation> _methodSimulations = [];

        protected TypeSimulation(TypeReference typeRef, ModuleDefinition moduleDef) : base(moduleDef)
        {
            Ref = typeRef.GetElementType().ImportInto(moduleDef);
            Def = typeRef.Resolve();
        }

        public TypeReference Ref { get; private set; }
        public TypeDefinition Def { get; }

        public TypeSimulation ReplaceGenerics(Dictionary<string, GenericParameter> genericMap)
        {
            Ref = Ref.ReplaceGenericArgs(genericMap);

            return this;
        }

        public TypeSimulation SetGenerics(TypeReference[] generics)
        {
            if (Ref is not GenericInstanceType git) throw new RougamoException($"Cannot set generic parameters for {Ref}, it is not a generic type.");
            if (git.GenericArguments.Count != generics.Length) throw new RougamoException($"Cannot set generic parameters for {Ref}, given generic parameters [{string.Join(",", generics.Select(x => x.ToString()))}]");

            git = new GenericInstanceType(Ref.GetElementType());
            git.GenericArguments.Add(generics);
            Ref = git;

            return this;
        }

        protected MethodSimulation<TRet> MethodSimulate<TRet>(string methodName) where TRet : TypeSimulation => MethodSimulate<TRet>(methodName, x => x.Name == methodName);

        protected MethodSimulation<TRet> MethodSimulate<TRet>(string id, Func<MethodDefinition, bool> predicate) where TRet : TypeSimulation
        {
            if (!_methodSimulations.TryGetValue(id, out var simulation))
            {
                simulation = Def.Methods.Single(predicate).Simulate<MethodSimulation<TRet>>(this);
                _methodSimulations[id] = simulation;
            }
            return (MethodSimulation<TRet>)simulation;
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

        public class PropertySimulation<T>(string propertyName, TypeSimulation declaringType) where T : TypeSimulation
        {
            protected readonly TypeSimulation _declaringType = declaringType;

            public string Name { get; } = propertyName;

            public MethodSimulation<T> Getter => _declaringType.MethodSimulate<T>(Constants.Getter(Name));

            public MethodSimulation<TypeSimulation> Setter => _declaringType.MethodSimulate<TypeSimulation>(Constants.Setter(Name));
        }
    }

    internal static class TypeSimulationExtensions
    {
        private static readonly Dictionary<Type, Func<TypeReference, ModuleDefinition, object>> _Cache = [];

        public static T Simulate<T>(this TypeReference typeRef, ModuleDefinition moduleDef) where T : TypeSimulation
        {
            var type = typeof(T);

            if (!_Cache.TryGetValue(type, out var ctor))
            {
                var ctorInfo = type.GetConstructor([typeof(TypeReference), typeof(ModuleDefinition)]);
                var psExp = ctorInfo.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name));
                var ctorExp = Expression.New(ctorInfo, psExp);
                ctor = Expression.Lambda<Func<TypeReference, ModuleDefinition, object>>(ctorExp, psExp).Compile();

                _Cache[type] = ctor;
            }

            return (T)ctor(typeRef, moduleDef);
        }
    }
}
