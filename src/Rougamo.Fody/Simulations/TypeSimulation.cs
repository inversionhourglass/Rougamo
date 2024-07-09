using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Concurrent;
using Mono.Cecil.Cil;

namespace Rougamo.Fody.Simulations
{
    internal class TypeSimulation : Simulation, IHost
    {
        private readonly Dictionary<string, FieldSimulation> _fieldSimulations = [];
        private readonly Dictionary<string, MethodSimulation> _methodSimulations = [];

        protected TypeSimulation(TypeReference typeRef, IHost? host, ModuleDefinition moduleDef) : base(moduleDef)
        {
            Ref = typeRef.ImportInto(moduleDef);
            Def = typeRef.Resolve();
            Host = host ?? new This(typeRef);
        }

        public TypeReference Ref { get; set; }

        public TypeDefinition Def { get; }

        public IHost Host { get; }

        public bool IsValueType => Ref.IsValueType;

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

        protected MethodSimulation<TRet> PublicMethodSimulate<TRet>(string methodName) where TRet : TypeSimulation => MethodSimulate<TRet>(methodName, x => x.Name == methodName && x.IsPublic);

        protected MethodSimulation MethodSimulate(string methodName) => MethodSimulate(methodName, x => x.Name == methodName);

        protected MethodSimulation MethodSimulate(string id, Func<MethodDefinition, bool> predicate)
        {
            if (!_methodSimulations.TryGetValue(id, out var simulation))
            {
                simulation = Def.Methods.Single(predicate).Simulate<MethodSimulation>(this);
                _methodSimulations[id] = simulation;
            }
            return simulation;
        }

        protected MethodSimulation PublicMethodSimulate(string methodName) => MethodSimulate(methodName, x => x.Name == methodName && x.IsPublic);

        protected T FieldSimulate<T>(string fieldName) where T : FieldSimulation => FieldSimulate<T>(fieldName, x => x.Name == fieldName);

        protected T FieldSimulate<T>(string id, Func<FieldDefinition, bool> predicate) where T : FieldSimulation
        {
            if (!_fieldSimulations.TryGetValue(id, out var simulation))
            {
                simulation = Def.Fields.Single(predicate).Simulate<T>(this);
                _fieldSimulations[id] = simulation;
            }
            return (T)simulation;
        }

        public virtual Instruction[]? LoadForCallingMethod() => Host.LoadForCallingMethod();

        public virtual Instruction[]? PrepareLoad(MethodSimulation method) => Host.PrepareLoad(method);

        public virtual Instruction[]? PrepareLoadAddress(MethodSimulation method) => Host.PrepareLoadAddress(method);

        public virtual Instruction[] Load(MethodSimulation method) => Host.Load(method);

        public virtual Instruction[] LoadAddress(MethodSimulation method) => Host.LoadAddress(method);

        public static implicit operator TypeReference(TypeSimulation value) => value.Ref;

        public class PropertySimulation(string propertyName, TypeSimulation declaringType)
        {
            protected readonly TypeSimulation _declaringType = declaringType;

            public string Name { get; } = propertyName;

            public MethodSimulation Getter => _declaringType.MethodSimulate(Constants.Getter(Name));

            public MethodSimulation Setter => _declaringType.MethodSimulate(Constants.Setter(Name));
        }

        public class PropertySimulation<T>(string propertyName, TypeSimulation declaringType) where T : TypeSimulation
        {
            protected readonly TypeSimulation _declaringType = declaringType;

            public string Name { get; } = propertyName;

            public MethodSimulation<T> Getter => _declaringType.MethodSimulate<T>(Constants.Getter(Name));

            public MethodSimulation Setter => _declaringType.MethodSimulate(Constants.Setter(Name));
        }
    }

    internal static class TypeSimulationExtensions
    {
        private static readonly ConcurrentDictionary<Type, Func<TypeReference, IHost?, ModuleDefinition, object>> _Cache = [];

        public static T Simulate<T>(this TypeReference typeRef, ModuleDefinition moduleDef) where T : TypeSimulation => Simulate<T>(typeRef, null, moduleDef);

        public static T Simulate<T>(this TypeReference typeRef, IHost? host, ModuleDefinition moduleDef) where T : TypeSimulation
        {
            var ctor = _Cache.GetOrAdd(typeof(T), t =>
            {
                var ctorInfo = t.GetConstructor([typeof(TypeReference), typeof(IHost), typeof(ModuleDefinition)]);
                return (tr, h, md) => ctorInfo.Invoke([tr, h, md]);
            });

            return (T)ctor(typeRef, host, moduleDef);
        }

        public static T ReplaceGenerics<T>(this T simulation, Dictionary<string, GenericParameter> genericMap) where T : TypeSimulation
        {
            simulation.Ref = simulation.Ref.ReplaceGenericArgs(genericMap);

            return simulation;
        }

        public static T SetGenerics<T>(this T simulation, TypeReference[] generics) where T : TypeSimulation
        {
            if (simulation.Ref is not GenericInstanceType git) throw new RougamoException($"Cannot set generic parameters for {simulation.Ref}, it is not a generic type.");
            if (git.GenericArguments.Count != generics.Length) throw new RougamoException($"Cannot set generic parameters for {simulation.Ref}, given generic parameters [{string.Join(",", generics.Select(x => x.ToString()))}]");

            git = new GenericInstanceType(simulation.Ref.GetElementType());
            git.GenericArguments.Add(generics);
            simulation.Ref = git;

            return simulation;
        }
    }
}
