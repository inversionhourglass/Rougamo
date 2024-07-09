using Mono.Cecil.Cil;
using Mono.Cecil;
using System.Collections.Concurrent;
using System;

namespace Rougamo.Fody.Simulations
{
    internal class VariableSimulation(VariableDefinition variableDef, ModuleDefinition moduleDef) : Simulation(moduleDef), IHost
    {
        public VariableDefinition VariableDef { get; } = variableDef;

        public Instruction[]? LoadForCallingMethod() => [VariableDef.LdlocAny()];

        public Instruction[]? PrepareLoad(MethodSimulation method) => null;

        public Instruction[]? PrepareLoadAddress(MethodSimulation method) => null;

        public Instruction[] Load(MethodSimulation method) => [VariableDef.Ldloc()];

        public Instruction[] LoadAddress(MethodSimulation method) => [VariableDef.Ldloca()];

        public static implicit operator VariableDefinition(VariableSimulation value) => value.VariableDef;
    }

    internal class VariableSimulation<T>(VariableDefinition variableDef, ModuleDefinition moduleDef) : VariableSimulation(variableDef, moduleDef) where T : TypeSimulation
    {
        private T? _value;

        public T Value => _value ??= VariableDef.VariableType.Simulate<T>(this, Module);
    }

    internal static class VariableSimulationExtensions
    {
        private static readonly ConcurrentDictionary<Type, Func<VariableDefinition, ModuleDefinition, object>> _Cache = [];

        public static T Simulate<T>(this VariableDefinition typeRef, ModuleDefinition moduleDef) where T : TypeSimulation
        {
            var ctor = _Cache.GetOrAdd(typeof(T), t =>
            {
                var ctorInfo = t.GetConstructor([typeof(VariableDefinition), typeof(ModuleDefinition)]);
                return (vd, md) => ctorInfo.Invoke([vd, md]);
            });

            return (T)ctor(typeRef, moduleDef);
        }
    }
}
