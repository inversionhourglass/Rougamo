using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Diagnostics;

namespace Fody.Simulations.PlainValues
{
    [DebuggerDisplay("Value = {value}, Type = {Type}")]
    public class ObjectValue(object value, TypeReference valueTypeRef, SimulationModuleWeaver moduleWeaver) : PlainValueSimulation(moduleWeaver), IParameterSimulation
    {
        public override TypeSimulation Type => valueTypeRef.Simulate(ModuleWeaver);

        public override IList<Instruction> Load()
        {
            return ModuleWeaver.LoadValueOnStack(valueTypeRef, value);
        }
    }

    public static class ObjectValueExtensions
    {
        public static ObjectValue Simulate(this CustomAttributeArgument attributeArgument, SimulationModuleWeaver moduleWeaver)
        {
            return new(attributeArgument.Value, attributeArgument.Type, moduleWeaver);
        }
    }
}
