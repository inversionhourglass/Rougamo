using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Simulations
{
    internal class MethodSimulation(TypeSimulation declaringType, MethodDefinition methodDef) : Simulation(declaringType.Module)
    {
        public TypeSimulation DeclaringType { get; } = declaringType;

        public MethodDefinition Def { get; } = methodDef;

        public MethodReference Ref { get; } = methodDef.WithGenericDeclaringType(declaringType);

        public VariableDefinition? TempThis { get; set; }

        public virtual IList<Instruction> Call(TypeSimulation[]? generics, params IParameterSimulation[] parameters)
        {
            if (Def.Parameters.Count != parameters.Length) throw new RougamoException($"Parameters count not match of method {Def}, need {Def.Parameters.Count} gave {parameters.Length}");

            var instructions = new List<Instruction>();

            var methodRef = generics == null ? Ref : Ref.WithGenerics(generics.Select(x => x.Ref).ToArray());

            for (var i = 0; i < parameters.Length; i++)
            {
                if (Def.Parameters[i].ParameterType is ByReferenceType)
                {
                    instructions.Add(parameters[i].PrepareLoadAddress(this));
                }
                else
                {
                    instructions.Add(parameters[i].PrepareLoad(this));
                }
            }

            instructions.Add(DeclaringType.LoadForCallingMethod());
            for (var i = 0; i < parameters.Length; i++)
            {
                if (Def.Parameters[i].ParameterType is ByReferenceType)
                {
                    instructions.Add(parameters[i].LoadAddress(this));
                }
                else
                {
                    instructions.Add(parameters[i].Load(this));
                }
            }
            instructions.Add(methodRef.CallAny());

            return instructions;
        }

        public static implicit operator MethodReference(MethodSimulation value) => value.Ref;
    }

    internal class MethodSimulation<T>(TypeSimulation declaringType, MethodDefinition methodDef) : MethodSimulation(declaringType, methodDef) where T : TypeSimulation
    {
        private T? _returnType;

        public T ReturnType => _returnType ??= Def.ReturnType.Simulate<T>(Module);
    }

    internal static class MethodSimulationExtensions
    {
        private static readonly ConcurrentDictionary<Type, Func<TypeSimulation, MethodDefinition, object>> _Cache = [];

        public static T Simulate<T>(this MethodDefinition methodDef, TypeSimulation declaringType) where T : MethodSimulation
        {
            var ctor = _Cache.GetOrAdd(typeof(T), t =>
            {
                var ctorInfo = t.GetConstructor([typeof(TypeSimulation), typeof(MethodDefinition)]);
                return (ts, md) => ctorInfo.Invoke([ts, md]);
            });

            return (T)ctor(declaringType, methodDef);
        }
    }
}
