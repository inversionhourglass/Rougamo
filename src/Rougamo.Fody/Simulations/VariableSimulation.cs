using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations
{
    internal class VariableSimulation(VariableDefinition variableDef, MethodSimulation declaringMethod) : Simulation(declaringMethod.Module), IHost, IAssignable
    {
        public VariableDefinition VariableDef { get; } = variableDef;

        public MethodSimulation DeclaringMethod { get; } = declaringMethod;

        public TypeReference TypeRef => VariableDef.VariableType;

        public IList<Instruction> LoadForCallingMethod() => [VariableDef.LdlocAny()];

        public IList<Instruction> PrepareLoadAddress(MethodSimulation method) => [];

        public IList<Instruction> LoadAddress(MethodSimulation method) => [VariableDef.Ldloca()];

        public IList<Instruction> Load() => [VariableDef.Ldloc()];

        public IList<Instruction> Assign(Func<IAssignable, IList<Instruction>> valueFactory)
        {
            return [.. valueFactory(this), VariableDef.Stloc()];
        }

        public IList<Instruction> AssignNew(TypeSimulation type, params IParameterSimulation[] arguments)
        {
            if (VariableDef.VariableType.IsValueType)
            {
                return [VariableDef.Ldloca(), .. type.New(arguments)];
            }
            return Assign(target => type.New(arguments));
        }

        public static implicit operator VariableDefinition(VariableSimulation value) => value.VariableDef;
    }

    internal class VariableSimulation<T>(VariableDefinition variableDef, MethodSimulation declaringMethod) : VariableSimulation(variableDef, declaringMethod) where T : TypeSimulation
    {
        private T? _value;

        public T Value => _value ??= VariableDef.VariableType.Simulate<T>(this, Module);

        public IList<Instruction> AssignNew(params IParameterSimulation[] arguments)
        {
            return AssignNew(Value, arguments);
        }
    }

    internal static class VariableSimulationExtensions
    {
        public static VariableSimulation Simulate(this VariableDefinition variableDef, MethodSimulation declaringMethod)
        {
            return new VariableSimulation(variableDef, declaringMethod);
        }

        public static VariableSimulation<T> Simulate<T>(this VariableDefinition typeRef, MethodSimulation declaringMethod) where T : TypeSimulation
        {
            return new VariableSimulation<T>(typeRef, declaringMethod);
        }
    }
}
