using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rougamo.Fody.Simulations
{
    [DebuggerDisplay("{VariableDef}")]
    internal class VariableSimulation(VariableDefinition variableDef, MethodSimulation declaringMethod) : Simulation(declaringMethod.ModuleWeaver), IHost, IAssignable
    {
        public VariableDefinition VariableDef { get; } = variableDef;

        public MethodSimulation DeclaringMethod { get; } = declaringMethod;

        public TypeSimulation Type { get; } = variableDef.VariableType.Simulate(declaringMethod.ModuleWeaver);

        public OpCode TrueToken => Type.TrueToken;

        public OpCode FalseToken => Type.FalseToken;

        public IList<Instruction> LoadForCallingMethod() => [VariableDef.LdlocAny()];

        public IList<Instruction> PrepareLoadAddress(MethodSimulation? method) => [];

        public IList<Instruction> LoadAddress(MethodSimulation? method) => [VariableDef.Ldloca()];

        public IList<Instruction> Load() => [VariableDef.Ldloc()];

        public IList<Instruction> Assign(Func<IAssignable, IList<Instruction>> valueFactory)
        {
            return [.. valueFactory(this), VariableDef.Stloc()];
        }

        public IList<Instruction> AssignNew(TypeSimulation type, params IParameterSimulation[] arguments)
        {
            if (VariableDef.VariableType.IsValueType)
            {
                return [VariableDef.Ldloca(), .. type.New(DeclaringMethod, arguments)];
            }
            return Assign(target => type.New(DeclaringMethod, arguments));
        }

        public IList<Instruction> AssignDefault(TypeSimulation type)
        {
            var varTypeRef = VariableDef.VariableType;
            if (varTypeRef.IsValueType || varTypeRef.IsGenericParameter)
            {
                return [VariableDef.Ldloca(), .. type.Default()];
            }
            return Assign(target => type.Default());
        }

        public IList<Instruction> Cast(TypeReference to) => Type.Cast(to);

        public static implicit operator VariableDefinition(VariableSimulation value) => value.VariableDef;
    }

    internal class VariableSimulation<T>(VariableDefinition variableDef, MethodSimulation declaringMethod) : VariableSimulation(variableDef, declaringMethod) where T : TypeSimulation
    {
        private T? _value;

        public T Value => _value ??= VariableDef.VariableType.Simulate<T>(this, ModuleWeaver);

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
