using Mono.Cecil;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System;
using Mono.Cecil.Cil;

namespace Rougamo.Fody.Simulations
{
    internal class FieldSimulation(TypeSimulation declaringType, FieldDefinition fieldDef) : Simulation(declaringType.Module), IHost
    {
        protected readonly TypeSimulation _declaringType = declaringType;

        public FieldDefinition FieldDef { get; } = fieldDef;

        public FieldReference FieldRef { get; } = new FieldReference(fieldDef.Name, fieldDef.FieldType, declaringType);

        public Instruction[]? LoadForCallingMethod()
        {
            return [.. _declaringType.LoadForCallingMethod(), FieldRef.LdfldAny()];
        }

        public Instruction[]? PrepareLoad(MethodSimulation method) => null;

        public Instruction[]? PrepareLoadAddress(MethodSimulation method) => null;

        public Instruction[] Load(MethodSimulation method)
        {
            return [.. _declaringType.Load(method), FieldRef.Ldfld()];
        }

        public Instruction[] LoadAddress(MethodSimulation method)
        {
            return [.. _declaringType.Load(method), FieldRef.Ldflda()];
        }

        public static implicit operator FieldDefinition(FieldSimulation value) => value.FieldDef;
    }

    internal class FieldSimulation<T>(TypeSimulation declaringType, FieldDefinition fieldDef) : FieldSimulation(declaringType, fieldDef) where T : TypeSimulation
    {
        private T? _value;

        public T Value => _value ??= FieldRef.FieldType.Simulate<T>(this, Module);
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
