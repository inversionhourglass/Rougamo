using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations
{
    internal class PropertySimulation(TypeSimulation declaringType, PropertyDefinition propertyDef) : Simulation(declaringType.Module), ILoadable
    {
        protected readonly TypeSimulation _declaringType = declaringType;

        public PropertyDefinition PropertyDef { get; } = propertyDef;

        public MethodSimulation? Getter { get; } = propertyDef.GetMethod?.Simulate(declaringType);

        public MethodSimulation? Setter { get; } = propertyDef.SetMethod?.Simulate(declaringType);

        public TypeReference TypeRef => PropertyDef.PropertyType;

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
