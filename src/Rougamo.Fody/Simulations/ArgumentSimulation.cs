using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Simulations
{
    internal class ArgumentSimulation(ParameterDefinition parameterDef, ModuleWeaver moduleWeaver) : Simulation(moduleWeaver), ILoadable, IParameterSimulation, IAssignable
    {
        public ParameterDefinition Def => parameterDef;

        public TypeSimulation Type { get; } = parameterDef.ParameterType is ByReferenceType refType ? refType.ElementType.Simulate(moduleWeaver) : parameterDef.ParameterType.Simulate(moduleWeaver);

        public bool IsByReference => parameterDef.ParameterType.IsByReference;

        public OpCode TrueToken => Type.TrueToken;

        public OpCode FalseToken => Type.FalseToken;

        public IList<Instruction> Assign(Func<IAssignable, IList<Instruction>> valueFactory)
        {
            if (IsByReference)
            {
                return [Create(OpCodes.Ldarg, parameterDef), .. valueFactory(this), Type.Ref.Stind()];
            }
            return [.. valueFactory(this), Create(OpCodes.Starg, parameterDef)];
        }

        public IList<Instruction> AssignDefault(TypeSimulation type)
        {
            var argTypeRef = Type.Ref;
            if (argTypeRef.IsValueType || argTypeRef.IsGenericParameter)
            {
                return [Create(IsByReference ? OpCodes.Ldarg : OpCodes.Ldarga, parameterDef), Create(OpCodes.Initobj, argTypeRef)];
            }
            if (IsByReference)
            {
                return [Create(OpCodes.Ldarg, parameterDef), Create(OpCodes.Ldnull), argTypeRef.Stind()];
            }
            return [Create(OpCodes.Ldnull), Create(OpCodes.Starg, parameterDef)];
        }

        public IList<Instruction> Load()
        {
            if (!IsByReference)
            {
                return [Create(OpCodes.Ldarg, parameterDef)];
            }
            return [Create(OpCodes.Ldarg, parameterDef), Type.Ref.Ldind()];
        }

        public IList<Instruction> Cast(TypeReference to)
        {
            return Type.Cast(to);
        }

        public IList<Instruction> PrepareLoadAddress(MethodSimulation? method)
        {
            return [];
        }

        public IList<Instruction> LoadAddress(MethodSimulation? method)
        {
            return [Create(OpCodes.Ldarg, parameterDef)];
        }
    }

    internal static class ArgumentSimulationExtensions
    {
        public static ArgumentSimulation Simulate(this ParameterDefinition parameterDef, ModuleWeaver moduleWeaver)
        {
            return new(parameterDef, moduleWeaver);
        }

        public static IList<Instruction> AssignDefault(this ArgumentSimulation arg) => arg.AssignDefault(arg.Type);
    }
}
