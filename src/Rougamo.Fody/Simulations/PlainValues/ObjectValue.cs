using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rougamo.Fody.Simulations.PlainValues
{
    [DebuggerDisplay("Value = {value}, Type = {Type}")]
    internal class ObjectValue(object value, TypeReference valueTypeRef, BaseModuleWeaver moduleWeaver) : PlainValueSimulation(moduleWeaver), IParameterSimulation
    {
        public override TypeSimulation Type => valueTypeRef.Simulate(ModuleWeaver);

        public override IList<Instruction> Load()
        {
            return ModuleWeaver.ModuleDefinition.LoadValueOnStack(valueTypeRef, value);
        }
    }

    internal static class ObjectValueExtensions
    {
        public static ObjectValue Simulate(this CustomAttributeArgument attributeArgument, BaseModuleWeaver moduleWeaver)
        {
            return new(attributeArgument.Value, attributeArgument.Type, moduleWeaver);
        }
    }
}
