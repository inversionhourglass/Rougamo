using Mono.Cecil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations
{
    internal abstract class MethodSimulation(TypeSimulation declaringType, MethodDefinition methodDef) : Simulation(declaringType.Module)
    {
        protected MethodReference? _ref;

        public TypeSimulation DeclaringType { get; } = declaringType;

        public MethodDefinition Def { get; } = methodDef;

        public MethodReference Ref => _ref ??= Def.WithGenericDeclaringType(DeclaringType);

        public static implicit operator MethodReference(MethodSimulation value) => value.Ref;
    }

    internal class MethodSimulation<T>(TypeSimulation declaringType, MethodDefinition methodDef) : MethodSimulation(declaringType, methodDef) where T : TypeSimulation
    {
        private T? _returnType;

        public T ReturnType => _returnType ??= Def.ReturnType.Simulate<T>(Module);

        public MethodSimulation<T> SetGenerics(params TypeReference[] generics)
        {
            _ref = Ref.WithGenerics(generics);

            return this;
        }
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
