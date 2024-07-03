using Mono.Cecil;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System;
using Mono.Cecil.Cil;

namespace Rougamo.Fody.Simulations
{
    internal class FieldSimulation : TypeSimulation
    {
        protected readonly TypeSimulation _declaringType;

        protected FieldSimulation(TypeSimulation declaringType, FieldDefinition fieldDef) : base(fieldDef.FieldType, declaringType.Module)
        {
            _declaringType = declaringType;
            FieldDef = fieldDef;
            FieldRef = new FieldReference(fieldDef.Name, fieldDef.FieldType, declaringType);
        }

        public FieldDefinition FieldDef { get; }

        public FieldReference FieldRef { get; }

        public override Instruction[]? LoadForCallingMethod()
        {
            return [.. _declaringType.LoadForCallingMethod(), FieldRef.LdfldAny()];
        }

        public override Instruction[]? PrepareLoad(MethodSimulation method) => null;

        public override Instruction[]? PrepareLoadAddress(MethodSimulation method) => null;

        public override Instruction[] Load(MethodSimulation method)
        {
            return [.. _declaringType.Load(method), FieldRef.Ldfld()];
        }

        public override Instruction[] LoadAddress(MethodSimulation method)
        {
            return [.. _declaringType.Load(method), FieldRef.Ldflda()];
        }

        public static implicit operator FieldDefinition(FieldSimulation value) => value.FieldDef;
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
