using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Simulations.PlainValues;
using System.Collections.Generic;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsArray(TypeReference elementTypeRef, IHost? host, BaseModuleWeaver moduleWeaver) : TypeSimulation(new ArrayType(elementTypeRef), host, moduleWeaver)
    {
        private readonly OpCode _ldCode = elementTypeRef.GetLdElemCode();
        private readonly OpCode _stCode = elementTypeRef.GetStElemCode();

        public TypeSimulation ElementType { get; } = elementTypeRef.Simulate(moduleWeaver);

        public Element this[int index]
        {
            get => new(this, index, _ldCode);
        } 

        public IList<Instruction> New(ILoadable[] items)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldc_I4, items.Length),
                Create(OpCodes.Newarr, ElementType)
            };
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                instructions.Add(Create(OpCodes.Dup));
                instructions.Add(Create(OpCodes.Ldc_I4, i));
                instructions.Add(item.Load());
                if (item.Type.IsValueType && !ElementType.IsValueType)
                {
                    instructions.Add(Create(OpCodes.Box, item.Type));
                }
                instructions.Add(Create(_stCode));
            }

            return instructions;
        }

        public Element Get(int index) => this[index];

        public class Element(TsArray array, int index, OpCode ldCode) : ILoadable
        {
            public TypeSimulation Type => array.ElementType;

            public OpCode TrueToken => Type.TrueToken;

            public OpCode FalseToken => Type.FalseToken;

            public IList<Instruction> Load()
            {
                return [.. array.Load(), Create(OpCodes.Ldc_I4, index), Create(ldCode)];
            }

            IList<Instruction> ILoadable.Cast(TypeReference to) => Type.Cast(to);
        }
    }

    internal static class TsArrayExtensions
    {
        public static RawValue NewAsPlainValue(this TsArray array, ILoadable[] items)
        {
            return new(array.Type, array.New(items));
        }
    }
}
