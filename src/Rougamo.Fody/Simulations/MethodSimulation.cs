using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Rougamo.Fody.Simulations
{
    internal abstract class MethodSimulation(TypeSimulation declaringType, MethodDefinition methodDef) : Simulation(declaringType.Module)
    {
        protected readonly TypeSimulation _declaringType = declaringType;
        private MethodReference? _ref;

        public MethodDefinition Def { get; } = methodDef;

        public MethodReference Ref => _ref ??= Def.WithGenericDeclaringType(_declaringType);

        public static implicit operator MethodDefinition(MethodSimulation value) => value.Def;
    }

    internal class MethodSimulation<T>(TypeSimulation declaringType, MethodDefinition methodDef) : MethodSimulation(declaringType, methodDef) where T : TypeSimulation
    {
        private T? _returnType;

        public T ReturnType => _returnType ??= Def.ReturnType.Simulate<T>(Module);
    }

    internal static class MethodSimulationExtensions
    {
        private static readonly Dictionary<Type, Func<TypeSimulation, MethodDefinition, object>> _Cache = [];

        public static T Simulate<T>(this MethodDefinition methodDef, TypeSimulation declaringType) where T : MethodSimulation
        {
            var type = typeof(T);

            if (!_Cache.TryGetValue(type, out var ctor))
            {
                var ctorInfo = type.GetConstructor([typeof(TypeSimulation), typeof(MethodDefinition)]);
                var psExp = ctorInfo.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name));
                var ctorExp = Expression.New(ctorInfo, psExp);
                ctor = Expression.Lambda<Func<TypeSimulation, MethodDefinition, object>>(ctorExp, psExp).Compile();

                _Cache[type] = ctor;
            }

            return (T)ctor(declaringType, methodDef);
        }
    }
}
