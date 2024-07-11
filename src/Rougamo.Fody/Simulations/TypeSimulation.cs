using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Concurrent;
using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.Instruction;
using Mono.Cecil.Rocks;

namespace Rougamo.Fody.Simulations
{
    internal class TypeSimulation : Simulation, IHost
    {
        private readonly Dictionary<string, object?> _fieldSimulations = [];
        private readonly Dictionary<string, MethodSimulation> _methodSimulations = [];
        private readonly Dictionary<string, PropertySimulation?> _propertySimulations = [];

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

        public TypeReference TypeRef => Ref;

        public virtual IList<Instruction> New(params IParameterSimulation?[] arguments)
        {
            if (IsValueType && arguments.Length == 0)
            {
                return [Create(OpCodes.Initobj, Ref)];
            }
            // todo: 考虑泛型参数问题
            var ctorDef = Def.GetConstructors().Single(x => x.Parameters.Count == arguments.Length && x.Parameters.Select(y => y.ParameterType.FullName).SequenceEqual(arguments.Select(y => y.TypeRef.FullName)));
            var ctorRef = ctorDef.ImportInto(Module).WithGenericDeclaringType(Ref);
            return ctorDef.Simulate(this).Call(null, arguments);
        }

        public virtual IList<Instruction> LoadForCallingMethod() => Host.LoadForCallingMethod();

        public virtual IList<Instruction> PrepareLoadAddress(MethodSimulation method) => Host.PrepareLoadAddress(method);

        public virtual IList<Instruction> LoadAddress(MethodSimulation method) => Host.LoadAddress(method);

        public IList<Instruction> Load() => Host.Load();

        #region Simulate

        protected MethodSimulation<TRet> MethodSimulate<TRet>(string methodName) where TRet : TypeSimulation => MethodSimulate<TRet>(methodName, x => x.Name == methodName);

        protected MethodSimulation<TRet> MethodSimulate<TRet>(string id, Func<MethodDefinition, bool> predicate) where TRet : TypeSimulation
        {
            if (!_methodSimulations.TryGetValue(id, out var simulation))
            {
                simulation = Def.Methods.Single(predicate).Simulate<TRet>(this);
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
                simulation = Def.Methods.Single(predicate).Simulate(this);
                _methodSimulations[id] = simulation;
            }
            return simulation;
        }

        protected MethodSimulation PublicMethodSimulate(string methodName) => MethodSimulate(methodName, x => x.Name == methodName && x.IsPublic);

        protected FieldSimulation<T> FieldSimulate<T>(string fieldName) where T : TypeSimulation => FieldSimulate<T>(fieldName, x => x.Name == fieldName);

        protected FieldSimulation<T> FieldSimulate<T>(string id, Func<FieldDefinition, bool> predicate) where T : TypeSimulation
        {
            if (!_fieldSimulations.TryGetValue(id, out var simulation))
            {
                simulation = Def.Fields.Single(predicate).Simulate<T>(this);
                _fieldSimulations[id] = simulation;
            }
            return (FieldSimulation<T>)simulation!;
        }

        protected FieldSimulation<T>[] FieldSimulates<T>(string id, Func<FieldDefinition, bool> predicate) where T : TypeSimulation
        {
            if (!_fieldSimulations.TryGetValue(id, out var simulation))
            {
                simulation = Def.Fields.Where(predicate).Select(x => x.Simulate<T>(this)).ToArray();
                _fieldSimulations[id] = simulation;
            }
            return (FieldSimulation<T>[])simulation!;
        }

        protected FieldSimulation<T>? OptionalFieldSimulate<T>(string fieldName) where T : TypeSimulation => FieldSimulate<T>(fieldName, x => x.Name == fieldName);

        protected FieldSimulation<T>? OptionalFieldSimulate<T>(string id, Func<FieldDefinition, bool> predicate) where T : TypeSimulation
        {
            if (!_fieldSimulations.TryGetValue(id, out var simulation))
            {
                var field = Def.Fields.SingleOrDefault(predicate);
                simulation = field?.Simulate<T>(this);
                _fieldSimulations[id] = simulation;
            }
            return (FieldSimulation<T>?)simulation;
        }

        protected FieldSimulation<T>[]? OptionalFieldSimulates<T>(string id, Func<FieldDefinition, bool> predicate) where T : TypeSimulation
        {
            if (!_fieldSimulations.TryGetValue(id, out var simulation))
            {
                var simulations = Def.Fields.Where(predicate).Select(x => x.Simulate<T>(this)).ToArray();
                simulation = simulations.Length == 0 ? null : simulations;
                _fieldSimulations[id] = simulation;
            }
            return (FieldSimulation<T>[]?)simulation;
        }

        protected PropertySimulation PropertySimulate(string propertyName, bool recursion)
        {
            return PropertySimulateInner(propertyName, recursion, false)!;
        }

        protected PropertySimulation<T> PropertySimulate<T>(string propertyName, bool recursion) where T : TypeSimulation
        {
            return PropertySimulateInner<T>(propertyName, recursion, false)!;
        }

        protected PropertySimulation? OptionalPropertySimulate(string propertyName, bool recursion)
        {
            return PropertySimulateInner(propertyName, recursion, true);
        }

        protected PropertySimulation<T>? OptionalPropertySimulate<T>(string propertyName, bool recursion) where T : TypeSimulation
        {
            return PropertySimulateInner<T>(propertyName, recursion, true);
        }

        private PropertySimulation? PropertySimulateInner(string propertyName, bool recursion, bool optional)
        {
            if (!_propertySimulations.TryGetValue(propertyName, out var simulation))
            {
                var def = Def;
                PropertyDefinition? propertyDef;
                do
                {
                    propertyDef = def.Properties.SingleOrDefault(x => x.Name == propertyName);
                } while (propertyDef == null && recursion && (def = def.BaseType.Resolve()) != null);
                if (propertyDef == null && !optional) throw new RougamoException($"Cannot find property({propertyName}) from {Def.FullName}");

                simulation = propertyDef?.Simulate(this);
                _propertySimulations[propertyName] = simulation;
            }

            return simulation;
        }

        private PropertySimulation<T>? PropertySimulateInner<T>(string propertyName, bool recursion, bool optional) where T : TypeSimulation
        {
            if (!_propertySimulations.TryGetValue(propertyName, out var simulation))
            {
                var def = Def;
                PropertyDefinition? propertyDef;
                do
                {
                    propertyDef = def.Properties.SingleOrDefault(x => x.Name == propertyName);
                } while (propertyDef == null && recursion && (def = def.BaseType.Resolve()) != null);
                if (propertyDef == null && !optional) throw new RougamoException($"Cannot find property({propertyName}) from {Def.FullName}");

                simulation = propertyDef?.Simulate<T>(this);
                _propertySimulations[propertyName] = simulation;
            }

            return (PropertySimulation<T>?)simulation;
        }

        #endregion Simulate

        public static implicit operator TypeReference(TypeSimulation value) => value.Ref;
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
