using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Simulations.PlainValues;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Rougamo.Fody.Simulations
{
    [DebuggerDisplay("{Ref}")]
    internal class MethodSimulation : Simulation
    {
        private readonly int[][]? _genericParaIndexes;
        private readonly Dictionary<string, TypeReference> _declaringTypeGenericMap;
        protected Dictionary<string, TypeReference>? _lastCallMethodGenericMap;

        public MethodSimulation(TypeSimulation declaringType, MethodDefinition methodDef) : base(declaringType.ModuleWeaver)
        {
            DeclaringType = declaringType;
            Def = methodDef;
            Ref = ModuleWeaver.Import(methodDef).WithGenericDeclaringType(declaringType);
            _declaringTypeGenericMap = DeclaringType.Ref.GetGenericMap();
            _genericParaIndexes = GetGenericParameterIndexes();
        }

        public MethodSimulation(TypeSimulation declaringType, MethodReference methodRef) : base(declaringType.ModuleWeaver)
        {
            DeclaringType = declaringType;
            Def = methodRef.Resolve();
            Ref = methodRef;
            _declaringTypeGenericMap = DeclaringType.Ref.GetGenericMap();
            _genericParaIndexes = GetGenericParameterIndexes();
        }

        protected Dictionary<string, TypeReference> GenericMap
        {
            get
            {
                if (_lastCallMethodGenericMap == null) return _declaringTypeGenericMap;

                var merged = new Dictionary<string, TypeReference>(_declaringTypeGenericMap);
                merged.AddRange(_lastCallMethodGenericMap);
                return merged;
            }
        }

        public TypeSimulation DeclaringType { get; }

        public MethodDefinition Def { get; }

        public MethodReference Ref { get; }

        public VariableDefinition? TempThis { get; set; }

        public virtual IList<Instruction> DupCall(MethodSimulation? host, params IParameterSimulation?[] arguments)
        {
            return InferGenericCall(host, true, arguments);
        }

        public virtual IList<Instruction> Call(MethodSimulation? host, params IParameterSimulation?[] arguments)
        {
            return InferGenericCall(host, false, arguments);
        }

        private IList<Instruction> InferGenericCall(MethodSimulation? host, bool dupCalling, params IParameterSimulation?[] arguments)
        {
            var generics = ResolveGenerics(arguments);
            SetGenericMap(generics);

            return Call(host, dupCalling, generics, arguments);
        }

        public IList<Instruction> Call(MethodSimulation? host, bool dupCalling, TypeSimulation[] generics, params IParameterSimulation?[] arguments)
        {
            if (Def.Parameters.Count != arguments.Length) throw new RougamoException($"Parameters count not match of method {Def}, need {Def.Parameters.Count} gave {arguments.Length}");

            var instructions = new List<Instruction>();

            var methodRef = generics == null ? Ref : Ref.WithGenerics(generics.Select(x => x.Ref).ToArray());

            var genericMap = ResolveGenericMap(methodRef);
            for (var i = 0; i < arguments.Length; i++)
            {
                if (Def.Parameters[i].ParameterType is ByReferenceType)
                {
                    var argument = arguments[i] ?? new Null(ModuleWeaver);
                    instructions.Add(argument.PrepareLoadAddress(host));
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
                var argument = arguments[i] ?? new Null(ModuleWeaver);
                var parameterTypeRef = Def.Parameters[i].ParameterType;
                if (parameterTypeRef is ByReferenceType)
                {
                    instructions.Add(argument.LoadAddress(host));
                }
                else
                {
                    instructions.Add(argument.Load());
                    if (parameterTypeRef is GenericParameter gp)
                    {
                        parameterTypeRef = genericMap[gp.Name];
                    }
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

        public GenericMethodSimulation MakeGenericMethod(TypeSimulation[] generics) => new(DeclaringType, Ref, generics);

        private Dictionary<string, TypeReference> ResolveGenericMap(MethodReference methodRef)
        {
            var map = new Dictionary<string, TypeReference>();

            if (methodRef.DeclaringType is GenericInstanceType git)
            {
                var typeDef = git.ElementType;
                for (var i = 0; i < typeDef.GenericParameters.Count; i++)
                {
                    map[typeDef.GenericParameters[i].Name] = git.GenericArguments[i];
                }
            }

            if (methodRef is GenericInstanceMethod gim)
            {
                var methodDef = methodRef.GetElementMethod();
                for (var i = 0; i < methodDef.GenericParameters.Count; i++)
                {
                    map[methodDef.GenericParameters[i].Name] = gim.GenericArguments[i];
                }
            }

            return map;
        }

        private int[][]? GetGenericParameterIndexes()
        {
            if (!Def.HasGenericParameters) return [];

            var indexes = Enumerable.Repeat<int[]?>(null, Def.GenericParameters.Count).ToArray();
            var index = 0;
            var map = new Dictionary<GenericParameter, int>();
            foreach (var gp in Def.GenericParameters)
            {
                map.Add(gp, index++);
            }
            for (var i = 0; i < Def.Parameters.Count; i++)
            {
                var parameterType = Def.Parameters[i].ParameterType;
                if (parameterType is ByReferenceType brt) parameterType = brt.ElementType;

                int[] steps = [i];
                ResolveGenericIndex(parameterType, indexes, steps, map);
            }

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return indexes.Contains(null) ? null : indexes;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

        private void ResolveGenericIndex(TypeReference parameterType, int[]?[] indexes, int[] steps, Dictionary<GenericParameter, int> map)
        {
            if (parameterType is GenericParameter gp && map.TryGetValue(gp, out var index))
            {
                indexes[index] = steps;
            }

            if (parameterType is GenericInstanceType git)
            {
                for (var i = 0; i < git.GenericArguments.Count; i++)
                {
                    ResolveGenericIndex(git.GenericArguments[i], indexes, [.. steps, i], map);
                }
            }
        }

        private TypeSimulation[] ResolveGenerics(IParameterSimulation?[] arguments)
        {
            if (_genericParaIndexes == null) throw new RougamoException($"[{Def}] Not all method generic parameters are inferred from parameters, you have to specify all the generic parameters manually.");
            return _genericParaIndexes.Select(indexes =>
            {
                var arg = arguments[indexes[0]] ?? throw new RougamoException($"[{Def}] Cannot infer the generic type from a null value at parameter index {indexes}");
                var argType = arg.Type.Ref;
                for (var i = 1; i < indexes.Length; i++)
                {
                    if (argType is not GenericInstanceType git) throw new RougamoException($"[{Def}] Cannot infer the generic type from type {argType}");

                    argType = git.GenericArguments[indexes[i]];
                }
                return argType.Simulate(ModuleWeaver);
            }).ToArray();
        }

        protected void SetGenericMap(TypeSimulation[] generics)
        {
            if (generics.Length == 0) return;

            var map = new Dictionary<string, TypeReference>();

            var gim = Ref as GenericInstanceMethod;
            for (var i = 0; i < generics.Length; i++)
            {
                if (gim != null)
                {
                    map[gim.GenericArguments[i].Name] = generics[i].Ref;
                }
                else
                {
                    map[Def.GenericParameters[i].Name] = generics[i].Ref;
                }
            }

            _lastCallMethodGenericMap = map;
        }

        public static implicit operator MethodReference(MethodSimulation value) => value.Ref;
    }

    internal class MethodSimulation<T> : MethodSimulation where T : TypeSimulation
    {
        public MethodSimulation(TypeSimulation declaringType, MethodDefinition methodDef) : base(declaringType, methodDef) { }

        public MethodSimulation(TypeSimulation declaringType, MethodReference methodRef) : base(declaringType, methodRef) { }

        private T? _result;

        /// <summary>
        /// Notice: If result type contains a generic type that is declared by the method and you will use the result value later,
        /// then you should call MakeGenericMethod to get a GenericMethodSimulation and use the Result of the GenericMethodSimulation instead,
        /// or you will get a wrong result type.
        /// </summary>
        public T Result => _result ??= Ref.ReturnType.ReplaceGenericArgs(GenericMap).Simulate<T>(new EmptyHost(ModuleWeaver), ModuleWeaver);

        public new GenericMethodSimulation<T> MakeGenericMethod(TypeSimulation[] generics) => new(DeclaringType, Ref, generics);
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
