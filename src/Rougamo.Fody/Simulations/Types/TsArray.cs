using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Simulations.PlainValues;
using System;
using System.Collections.Generic;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsArray(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TsArray<TypeSimulation>(typeRef, host, moduleWeaver) { }

    internal class TsArray<T> : TypeSimulation, IDisposable where T : TypeSimulation
    {
        private readonly OpCode _ldCode;
        private readonly OpCode _stCode;
        private bool _dupScope;

        public TsArray(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : base(typeRef, host, moduleWeaver)
        {
            var arrayTypeRef = (ArrayType)typeRef;

            _ldCode = arrayTypeRef.ElementType.GetLdElemCode();
            _stCode = arrayTypeRef.ElementType.GetStElemCode();
            ElementType = arrayTypeRef.ElementType.Simulate<T>(moduleWeaver);
        }

        public T ElementType { get; }

        public Element this[int index] => this[new Int32Value(index, ModuleWeaver)];

        public Element this[ILoadable indexLoader] => new(this, indexLoader);

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
                instructions.Add(item.Cast(ElementType));
                instructions.Add(Create(_stCode));
            }

            return instructions;
        }

        public Element Get(int index) => this[index];

        public Element Get(ILoadable indexLoader) => this[indexLoader];

        public IDisposable EnterDupScope()
        {
            _dupScope = true;
            return this;
        }

        public override IList<Instruction> Load()
        {
            return _dupScope ? [Create(OpCodes.Dup)] : base.Load();
        }

        public void Dispose()
        {
            _dupScope = false;
        }

        public class Element : Simulation, IHost, IAssignable
        {
            private readonly TsArray<T> _array;
            private readonly ILoadable _indexLoader;

            public Element(TsArray<T> array, ILoadable indexLoader) : base(array.ModuleWeaver)
            {
                _array = array;
                _indexLoader = indexLoader;
                Type = array.ElementType;
                Value = array.ElementType.Ref.Simulate<T>(this, ModuleWeaver);
            }

            public TypeSimulation Type { get; }

            public OpCode TrueToken => Type.TrueToken;

            public OpCode FalseToken => Type.FalseToken;

            public T Value { get; }

            public IList<Instruction> Load()
            {
                return [.. _array.Load(), .. _indexLoader.Load(), Create(_array._ldCode)];
            }

            public IList<Instruction> Assign(Func<IAssignable, IList<Instruction>> valueFactory)
            {
                return [.. _array.Load(), .. _indexLoader.Load(), .. valueFactory(this), Create(_array._stCode)];
            }

            public IList<Instruction> Cast(TypeReference to) => Type.Cast(to);

            public IList<Instruction> LoadForCallingMethod()
            {
                var ldele = Type.IsValueType ? Create(OpCodes.Ldelema, Type) : Create(_array._ldCode);

                return [.. _array.Load(), .. _indexLoader.Load(), ldele];
            }

            public IList<Instruction> PrepareLoadAddress(MethodSimulation? method)
            {
                return [];
            }

            public IList<Instruction> LoadAddress(MethodSimulation? method)
            {
                return [.. _array.Load(), .. _indexLoader.Load(), Create(OpCodes.Ldelema, Type)];
            }
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
