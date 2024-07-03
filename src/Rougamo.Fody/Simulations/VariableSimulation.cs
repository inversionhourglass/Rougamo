using Mono.Cecil.Cil;
using Mono.Cecil;
using System.Collections.Concurrent;
using System;

namespace Rougamo.Fody.Simulations
{
    internal class VariableSimulation : TypeSimulation
    {
        protected VariableSimulation(VariableDefinition variableDef, ModuleDefinition moduleDef) : base(variableDef.VariableType, moduleDef)
        {
            VariableDef = variableDef;
        }

        public VariableDefinition VariableDef { get; }

        public override Instruction[]? LoadForCallingMethod()
        {
            return [VariableDef.LdlocAny()];
        }

        public override Instruction[]? PrepareLoad(MethodSimulation method) => null;

        public override Instruction[]? PrepareLoadAddress(MethodSimulation method) => null;

        public override Instruction[] Load(MethodSimulation method)
        {
            return [VariableDef.Ldloc()];
        }

        public override Instruction[] LoadAddress(MethodSimulation method)
        {
            return [VariableDef.Ldloca()];
        }

        public static implicit operator VariableDefinition(VariableSimulation value) => value.VariableDef;
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
