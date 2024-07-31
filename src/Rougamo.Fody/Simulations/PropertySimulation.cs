using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Simulations.PlainValues;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rougamo.Fody.Simulations
{
    [DebuggerDisplay("{PropertyDef}")]
    internal class PropertySimulation(TypeSimulation declaringType, PropertyDefinition propertyDef) : Simulation(declaringType.ModuleWeaver), ILoadable, IAssignable
    {
        protected readonly TypeSimulation _declaringType = declaringType;

        public PropertyDefinition PropertyDef { get; } = propertyDef;

        public MethodSimulation? Getter { get; } = propertyDef.GetMethod?.Simulate(declaringType);

        public MethodSimulation? Setter { get; } = propertyDef.SetMethod?.Simulate(declaringType);

        public TypeSimulation Type { get; } = declaringType.Ref.Import(propertyDef.PropertyType).Simulate(declaringType.ModuleWeaver);

        public OpCode TrueToken => Type.TrueToken;

        public OpCode FalseToken => Type.FalseToken;

        public IList<Instruction> Assign(Func<IAssignable, IList<Instruction>> valueFactory)
        {
            return Setter!.Call(null, new RawValue(Type, valueFactory(this)));
        }

        public IList<Instruction> Cast(TypeReference to) => Type.Cast(to);

        public IList<Instruction> Load() => Getter!.Call(null);

        public static implicit operator PropertyDefinition(PropertySimulation value) => value.PropertyDef;
    }

    internal class PropertySimulation<T>(TypeSimulation declaringType, PropertyDefinition propertyDef) : PropertySimulation(declaringType, propertyDef) where T : TypeSimulation
    {
        public new MethodSimulation<T>? Getter { get; } = propertyDef.GetMethod?.Simulate<T>(declaringType);
    }

    internal static class PropertySimulationExtensions
    {
        public static PropertySimulation Simulate(this PropertyDefinition propertyDef, TypeSimulation declaringType)
        {
            return new PropertySimulation(declaringType, propertyDef);
        }

        public static PropertySimulation<T> Simulate<T>(this PropertyDefinition propertyDef, TypeSimulation declaringType) where T : TypeSimulation
        {
            return new PropertySimulation<T>(declaringType, propertyDef);
        }
    }
}
