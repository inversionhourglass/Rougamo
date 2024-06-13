using Mono.Cecil;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System;

namespace Rougamo.Fody.Simulations
{
    internal class FieldSimulation : Simulation
    {
        protected readonly TypeSimulation _declaringType;

        protected FieldSimulation(TypeSimulation declaringType, FieldDefinition fieldDef) : base(declaringType.Module)
        {
            _declaringType = declaringType;
            Def = fieldDef;
            Ref = new FieldReference(fieldDef.Name, fieldDef.FieldType, declaringType);
        }

        public FieldDefinition Def { get; }

        public FieldReference Ref { get; }

        public static implicit operator FieldReference(FieldSimulation value) => value.Ref;
    }

    internal static class FieldSimulationExtensions
    {
        private static readonly Dictionary<Type, Func<TypeSimulation, FieldDefinition, object>> _Cache = [];

        public static T Simulate<T>(this FieldDefinition fieldDef, TypeSimulation declaringType) where T : FieldSimulation
        {
            var type = typeof(T);

            if (!_Cache.TryGetValue(type, out var ctor))
            {
                var ctorInfo = type.GetConstructor([typeof(TypeSimulation), typeof(FieldDefinition)]);
                var psExp = ctorInfo.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name));
                var ctorExp = Expression.New(ctorInfo, psExp);
                ctor = Expression.Lambda<Func<TypeSimulation, FieldDefinition, object>>(ctorExp, psExp).Compile();

                _Cache[type] = ctor;
            }

            return (T)ctor(declaringType, fieldDef);
        }
    }
}
