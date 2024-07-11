using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsArray(TypeReference elementTypeRef, IHost? host, ModuleDefinition moduleDef) : TypeSimulation(new ArrayType(elementTypeRef), host, moduleDef)
    {
        public TypeReference ElementTypeRef => elementTypeRef;

        public IList<Instruction> New(ILoadable[] items)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldc_I4, items.Length),
                Create(OpCodes.Newarr, Ref)
            };
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                instructions.Add(Create(OpCodes.Dup));
                instructions.Add(Create(OpCodes.Ldc_I4, i));
                instructions.Add(item.Load());
                if (item.TypeRef.IsValueType && !ElementTypeRef.IsValueType)
                {
                    instructions.Add(Create(OpCodes.Box, item.TypeRef));
                }
                instructions.Add(Create(ElementTypeRef.GetStElemCode()));
            }

            return instructions;
        }
    }
}
