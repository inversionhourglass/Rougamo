using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.PlainValues
{
    internal class ObjectValue(object value, TypeReference valueTypeRef, ModuleDefinition moduleDef) : PlainValueSimulation(moduleDef), IParameterSimulation
    {
        public override TypeSimulation Type => valueTypeRef.Simulate(Module);

        public override IList<Instruction> Load()
        {
            return Module.LoadValueOnStack(valueTypeRef, value);
        }
    }

    internal static class ObjectValueExtensions
    {
        public static ObjectValue Simulate(this CustomAttributeArgument attributeArgument, ModuleDefinition moduleDef)
        {
            return new ObjectValue(attributeArgument.Value, attributeArgument.Type, moduleDef);
        }
    }
}
