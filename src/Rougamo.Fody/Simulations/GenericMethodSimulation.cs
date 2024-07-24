using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations
{
    internal class GenericMethodSimulation : MethodSimulation
    {
        private readonly TypeSimulation[] _generics;

        public GenericMethodSimulation(TypeSimulation declaringType, MethodDefinition methodDef, TypeSimulation[] generics) : base(declaringType, methodDef)
        {
            SetGenericMap(generics);
            _generics = generics;
        }

        public GenericMethodSimulation(TypeSimulation declaringType, MethodReference methodRef, TypeSimulation[] generics) : base(declaringType, methodRef)
        {
            SetGenericMap(generics);
            _generics = generics;
        }

        public override IList<Instruction> Call(MethodSimulation? host, params IParameterSimulation?[] arguments)
        {
            return Call(host, false, _generics, arguments);
        }

        public override IList<Instruction> DupCall(MethodSimulation? host, params IParameterSimulation?[] arguments)
        {
            return Call(host, true, _generics, arguments);
        }
    }

    internal class GenericMethodSimulation<T> : MethodSimulation<T> where T : TypeSimulation
    {
        private readonly TypeSimulation[] _generics;

        public GenericMethodSimulation(TypeSimulation declaringType, MethodDefinition methodDef, TypeSimulation[] generics) : base(declaringType, methodDef)
        {
            SetGenericMap(generics);
            _generics = generics;
        }

        public GenericMethodSimulation(TypeSimulation declaringType, MethodReference methodRef, TypeSimulation[] generics) : base(declaringType, methodRef)
        {
            SetGenericMap(generics);
            _generics = generics;
        }

        public override IList<Instruction> Call(MethodSimulation? host, params IParameterSimulation?[] arguments)
        {
            return Call(host, false, _generics, arguments);
        }

        public override IList<Instruction> DupCall(MethodSimulation? host, params IParameterSimulation?[] arguments)
        {
            return Call(host, true, _generics, arguments);
        }
    }
}
