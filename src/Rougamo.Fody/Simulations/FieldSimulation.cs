using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Simulations
{
    [DebuggerDisplay("{FieldRef}")]
    internal class FieldSimulation(TypeSimulation declaringType, FieldDefinition fieldDef) : Simulation(declaringType.ModuleWeaver), IHost, IAssignable
    {
        protected readonly TypeSimulation _declaringType = declaringType;
        private TypeSimulation? _value;

        public FieldDefinition FieldDef { get; } = fieldDef;

        public FieldReference FieldRef { get; } = new FieldReference(fieldDef.Name, fieldDef.FieldType, declaringType);

        public TypeSimulation Type => Value;

        public TypeSimulation Value => _value ??= FieldRef.FieldType.Simulate(this, ModuleWeaver);

        public OpCode TrueToken => Type.TrueToken;

        public OpCode FalseToken => Type.FalseToken;

        public IList<Instruction> LoadForCallingMethod()
        {
            return [.. _declaringType.LoadForCallingMethod(), FieldRef.LdfldAny()];
        }

        public IList<Instruction> PrepareLoadAddress(MethodSimulation? method) => [];

        public IList<Instruction> LoadAddress(MethodSimulation? method)
        {
            return [.. _declaringType.Load(), FieldRef.Ldflda()];
        }

        public IList<Instruction> Load()
        {
            return [.. _declaringType.Load(), FieldRef.Ldfld()];
        }

        public IList<Instruction> Assign(Func<IAssignable, IList<Instruction>> valueFactory)
        {
            return [.. _declaringType.Load(), .. valueFactory(this), FieldRef.Stfld()];
        }

        public IList<Instruction> AssignNew(MethodSimulation host, TypeSimulation type, params IParameterSimulation?[] arguments)
        {
            if (FieldRef.FieldType.IsValueType)
            {
                return [.. _declaringType.Load(), FieldRef.Ldflda(), .. type.New(host, arguments)];
            }
            return Assign(target => type.New(host, arguments));
        }

        public IList<Instruction> AssignDefault(TypeSimulation type)
        {
            var filedTypeRef = FieldRef.FieldType;
            if (filedTypeRef.IsValueType || filedTypeRef.IsGenericParameter)
            {
                return [.. _declaringType.Load(), FieldRef.Ldflda(), Create(OpCodes.Initobj, Type)];
            }
            return [.. _declaringType.Load(), Create(OpCodes.Ldnull), FieldRef.Stfld()];
        }

        public IList<Instruction> Cast(TypeReference to) => Type.Cast(to);

        public static implicit operator FieldReference(FieldSimulation value) => value.FieldRef;
    }

    internal class FieldSimulation<T>(TypeSimulation declaringType, FieldDefinition fieldDef) : FieldSimulation(declaringType, fieldDef) where T : TypeSimulation
    {
        private T? _value;

        public new T Value => _value ??= FieldRef.FieldType.Simulate<T>(this, ModuleWeaver);

        public IList<Instruction> AssignNew(MethodSimulation host, params IParameterSimulation?[] arguments)
        {
            return AssignNew(host, Value, arguments);
        }
    }

    internal static class FieldSimulationExtensions
    {
        public static FieldSimulation Simulate(this FieldDefinition fieldDef, TypeSimulation declaringType)
        {
            return new FieldSimulation(declaringType, fieldDef);
        }

        public static FieldSimulation<T> Simulate<T>(this FieldDefinition fieldDef, TypeSimulation declaringType) where T : TypeSimulation
        {
            return new FieldSimulation<T>(declaringType, fieldDef);
        }

        public static IList<Instruction> AssignDefault(this FieldSimulation field) => field.AssignDefault(field.Value);

        public static IList<Instruction> AssignDefault<T>(this FieldSimulation<T> field) where T : TypeSimulation => field.AssignDefault(field.Value);
    }
}
