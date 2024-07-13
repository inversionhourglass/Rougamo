using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Simulations.PlainValues;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Simulations
{
    internal class MethodSimulation : Simulation
    {
        private readonly int[]? _genericParaIndexes;

        public MethodSimulation(TypeSimulation declaringType, MethodDefinition methodDef) : base(declaringType.Module)
        {
            DeclaringType = declaringType;
            Def = methodDef;
            Ref = methodDef.WithGenericDeclaringType(declaringType).ImportInto(Module);
            _genericParaIndexes = GetGenericParameterIndexes();
        }

        public MethodSimulation(TypeSimulation declaringType, MethodReference methodRef) : base(declaringType.Module)
        {
            DeclaringType = declaringType;
            Def = methodRef.Resolve();
            Ref = methodRef;
            _genericParaIndexes = GetGenericParameterIndexes();
        }

        public TypeSimulation DeclaringType { get; }

        public MethodDefinition Def { get; }

        public MethodReference Ref { get; }

        public VariableDefinition? TempThis { get; set; }

        public virtual IList<Instruction> DupCall(params IParameterSimulation?[] arguments)
        {
            return Call(true, null, arguments);
        }

        public virtual IList<Instruction> Call(params IParameterSimulation?[] arguments)
        {
            return Call(false, null, arguments);
        }

        public IList<Instruction> Call(bool dupCalling, TypeSimulation[]? generics, params IParameterSimulation?[] arguments)
        {
            if (Def.Parameters.Count != arguments.Length) throw new RougamoException($"Parameters count not match of method {Def}, need {Def.Parameters.Count} gave {arguments.Length}");

            var instructions = new List<Instruction>();

            if (generics == null)
            {
                if (_genericParaIndexes == null) throw new RougamoException($"[{Def}] Not all method generic parameters are inferred from parameters, you have to specify all the generic parameters manually.");
                generics = _genericParaIndexes.Select(x => arguments[x]?.Type ?? throw new RougamoException($"[{Def}] Cannot infer the generic type from a null value at parameter index {x}")).ToArray();
            }

            var methodRef = generics == null ? Ref : Ref.WithGenerics(generics.Select(x => x.Ref).ToArray());

            for (var i = 0; i < arguments.Length; i++)
            {
                if (Def.Parameters[i].ParameterType is ByReferenceType)
                {
                    var argument = arguments[i] ?? new Null();
                    instructions.Add(argument.PrepareLoadAddress(this));
                }
            }

            if (!Def.IsConstructor && !Def.IsStatic)
            {
                if (dupCalling)
                {
                    instructions.Add(Instruction.Create(OpCodes.Dup));
                }
                else
                {
                    instructions.Add(DeclaringType.LoadForCallingMethod());
                }
            }
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i] ?? new Null();
                var parameterTypeRef = Def.Parameters[i].ParameterType;
                if (parameterTypeRef is ByReferenceType)
                {
                    instructions.Add(argument.LoadAddress(this));
                }
                else
                {
                    instructions.Add(argument.Load());
                    instructions.Add(argument.Cast(parameterTypeRef));
                }
            }
            instructions.Add(methodRef.CallAny());

            return instructions;
        }

        public VariableSimulation CreateVariable(TypeReference variableTypeRef)
        {
            return Def.Body.CreateVariable(variableTypeRef).Simulate(this);
        }

        public VariableSimulation<T> CreateVariable<T>(TypeReference variableTypeRef) where T : TypeSimulation
        {
            return Def.Body.CreateVariable(variableTypeRef).Simulate<T>(this);
        }

        private int[]? GetGenericParameterIndexes()
        {
            if (!Def.HasGenericParameters) return [];

            var indexes = Enumerable.Repeat(-1, Def.GenericParameters.Count).ToArray();
            var index = 0;
            var map = new Dictionary<GenericParameter, int>();
            foreach (var gp in Def.GenericParameters)
            {
                map.Add(gp, index++);
            }
            for (var i = 0; i < Def.Parameters.Count; i++)
            {
                if (Def.Parameters[i].ParameterType is GenericParameter gp && map.TryGetValue(gp, out var idx))
                {
                    indexes[idx] = i;
                }
            }

            return indexes.Contains(-1) ? null : indexes;
        }

        public static implicit operator MethodReference(MethodSimulation value) => value.Ref;
    }

    internal class MethodSimulation<T> : MethodSimulation where T : TypeSimulation
    {
        public MethodSimulation(TypeSimulation declaringType, MethodDefinition methodDef) : base(declaringType, methodDef) { }

        public MethodSimulation(TypeSimulation declaringType, MethodReference methodRef) : base(declaringType, methodRef) { }

        private T? _result;

        public T Result => _result ??= Def.ReturnType.Simulate<T>(new EmptyHost(), Module);
    }

    internal static class MethodSimulationExtensions
    {
        public static MethodSimulation Simulate(this MethodDefinition methodDef, TypeSimulation declaringType)
        {
            return new MethodSimulation(declaringType, methodDef);
        }

        public static MethodSimulation Simulate(this MethodReference methodRef, TypeSimulation declaringType)
        {
            return new MethodSimulation(declaringType, methodRef);
        }

        public static MethodSimulation<T> Simulate<T>(this MethodDefinition methodDef, TypeSimulation declaringType) where T : TypeSimulation
        {
            return new MethodSimulation<T>(declaringType, methodDef);
        }

        public static MethodSimulation<T> Simulate<T>(this MethodReference methodRef, TypeSimulation declaringType) where T : TypeSimulation
        {
            return new MethodSimulation<T>(declaringType, methodRef);
        }
    }
}
