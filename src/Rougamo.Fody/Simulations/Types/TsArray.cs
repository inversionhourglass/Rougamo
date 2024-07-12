using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsArray(TypeReference elementTypeRef, IHost? host, ModuleDefinition moduleDef) : TypeSimulation(new ArrayType(elementTypeRef), host, moduleDef)
    {
        private readonly OpCode _ldCode = elementTypeRef.GetLdElemCode();
        private readonly OpCode _stCode = elementTypeRef.GetStElemCode();

        public TypeReference ElementTypeRef => elementTypeRef;

        public Element this[int index]
        {
            get => new(this, index, _ldCode);
        } 

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
                instructions.Add(Create(_stCode));
            }

            return instructions;
        }

        public Element Get(int index) => this[index];

        public class Element(TsArray array, int index, OpCode ldCode) : ILoadable
        {
            public TypeReference TypeRef => array.ElementTypeRef;

            public IList<Instruction> Load()
            {
                return [.. array.Load(), Create(OpCodes.Ldc_I4, index), Create(ldCode)];
            }
        }
    }
}
