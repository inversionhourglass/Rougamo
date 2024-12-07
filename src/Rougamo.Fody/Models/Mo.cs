using Fody;
using Fody.Simulations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using Rougamo.Context;
using Rougamo.Fody.Models;
using Rougamo.Fody.Simulations.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    internal abstract class Mo(TypeReference moTypeRef, MoFrom from)
    {
        private AccessFlags? _flags;
        private string? _pattern;
        private bool _patternSet;
        private Feature? _features;
        private double? _order;
        private Omit? _omit;
        private ForceSync? _forceSync;
        private Lifetime? _lifetime;

        public static IEqualityComparer<Mo> Comparer { get; } = new EqualityComparer();

        public MoFrom From => from;

        public abstract bool IsStruct { get; }

        public abstract bool HasArguments { get; }

        public abstract bool HasProperties { get; }

        public AccessFlags Flags
        {
            get
            {
                if (!_flags.HasValue)
                {
                    _flags = ExtractFlags();
                }
                return _flags.Value;
            }
        }

        public string? Pattern
        {
            get
            {
                if (!_patternSet)
                {
                    _pattern = ExtractPattern();
                    _patternSet = true;
                }
                return _pattern;
            }
        }

        public Feature Features
        {
            get
            {
                if (!_features.HasValue)
                {
                    _features = ExtractFeatures();
                }
                return _features.Value;
            }
        }

        public double Order
        {
            get
            {
                if (!_order.HasValue)
                {
                    _order = ExtractOrder();
                }
                return _order.Value;
            }
        }

        public Omit MethodContextOmits
        {
            get
            {
                if (!_omit.HasValue)
                {
                    _omit = ExtractOmits();
                }
                return _omit.Value;
            }
        }

        public ForceSync ForceSync
        {
            get
            {
                if (!_forceSync.HasValue)
                {
                    _forceSync = ExtractForceSync();
                }
                return _forceSync.Value;
            }
        }

        public Lifetime Lifetime
        {
            get
            {
                if (!_lifetime.HasValue)
                {
                    _lifetime = ExtractLifetime();
                }
                return _lifetime.Value;
            }
        }

        public TypeDefinition MoTypeDef { get; } = moTypeRef.ToDefinition();

        public TypeReference MoTypeRef => moTypeRef;

        public string FullName => moTypeRef.FullName;

        public abstract IList<Instruction> New(TsMo tMo, MethodSimulation host);

        protected virtual AccessFlags ExtractFlags()
        {
            var implementedInterface = MoTypeDef.Implement(Constants.TYPE_IFlexibleModifierPointcut);
            if (!implementedInterface && MoTypeDef.Properties.Any(x => x.Name == Constants.PROP_Flags && x.PropertyType.IsEnum(out var ptFlag) && ptFlag!.IsInt32()))
            {
                throw new FodyWeavingException($"[{MoTypeDef}] Since version 5.0.0, the Flags property has been removed from the IMo interface. Use PointcutAttribute instead. For more information, see https://github.com/inversionhourglass/Rougamo/releases/tag/v5.0.0");
            }
            var pointcutAttribute = MoTypeDef.CustomAttributes.FirstOrDefault(x => x.Is(Constants.TYPE_PointcutAttribute));
            if (pointcutAttribute != null && pointcutAttribute.ConstructorArguments.Count == 1)
            {
                var arg = pointcutAttribute.ConstructorArguments[0];
                if (!arg.Type.IsString())
                {
                    return (AccessFlags)Convert.ToInt32(arg.Value);
                }
            }
            AccessFlags? flags = null;
            if (implementedInterface)
            {
                flags = ExtractFromIl(MoTypeRef, Constants.PROP_Flags, Constants.TYPE_AccessFlags, ParseFlags);
            }
            return flags ?? AccessFlags.InstancePublic;

            static AccessFlags? ParseFlags(Instruction instruction)
            {
                var opCode = instruction.OpCode;
                if (opCode == OpCodes.Ldc_I4_3) return AccessFlags.Public;
                if (opCode == OpCodes.Ldc_I4_6) return AccessFlags.Instance;
                if (opCode == OpCodes.Ldc_I4_7) return AccessFlags.Instance | AccessFlags.Public;
                if (opCode == OpCodes.Ldc_I4_S || opCode == OpCodes.Ldc_I4) return (AccessFlags)Convert.ToInt32(instruction.Operand);
                return null;
            }
        }

        protected virtual string? ExtractPattern()
        {
            var implementedInterface = MoTypeDef.Implement(Constants.TYPE_IFlexiblePatternPointcut);
            if (!implementedInterface && MoTypeDef.Properties.Any(x => x.Name == Constants.PROP_Pattern && x.PropertyType.IsString()))
            {
                throw new FodyWeavingException($"[{MoTypeDef}] Since version 5.0.0, the Pattern property has been removed from the IMo interface. Use PointcutAttribute instead. For more information, see https://github.com/inversionhourglass/Rougamo/releases/tag/v5.0.0");
            }
            var pointcutAttribute = MoTypeDef.CustomAttributes.FirstOrDefault(x => x.Is(Constants.TYPE_PointcutAttribute));
            if (pointcutAttribute != null && pointcutAttribute.ConstructorArguments.Count == 1)
            {
                var arg = pointcutAttribute.ConstructorArguments[0];
                if (arg.Type.IsString())
                {
                    return (string)arg.Value;
                }
            }
            return implementedInterface ? ExtractFromIl(MoTypeRef, Constants.PROP_Pattern, Constants.TYPE_String, ParsePattern) : null;

            static string? ParsePattern(Instruction instruction)
            {
                return instruction.OpCode.Code == Code.Ldstr ? (string)instruction.Operand : null;
            }
        }

        protected virtual Feature ExtractFeatures()
        {
            if (MoTypeDef.Properties.Any(x => x.Name == Constants.PROP_Features && x.PropertyType.IsEnum(out var ptFeature) && ptFeature!.IsInt32()))
            {
                throw new FodyWeavingException($"[{MoTypeDef}] Since version 5.0.0, the Features property has been removed from the IMo interface. Use AdviceAttribute instead. For more information, see https://github.com/inversionhourglass/Rougamo/releases/tag/v5.0.0");
            }
            var adviceAttribute = MoTypeDef.CustomAttributes.FirstOrDefault(x => x.Is(Constants.TYPE_AdviceAttribute));
            if (adviceAttribute != null && adviceAttribute.ConstructorArguments.Count == 1)
            {
                var arg = adviceAttribute.ConstructorArguments[0];
                return (Feature)Convert.ToInt32(arg.Value);
            }
            return Feature.All;
        }

        protected virtual double ExtractOrder()
        {
            var implementedInterface = MoTypeDef.Implement(Constants.TYPE_IFlexibleOrderable);
            if (!implementedInterface && MoTypeDef.Properties.Any(x => x.Name == Constants.PROP_Order && x.PropertyType.IsDouble()))
            {
                throw new FodyWeavingException($"[{MoTypeDef}] Since version 5.0.0, the Order property has been removed from the IMo interface. Implements IFlexibleOrderable instead. For more information, see https://github.com/inversionhourglass/Rougamo/releases/tag/v5.0.0");
            }
            if (implementedInterface)
            {
                var order = ExtractFromIl(MoTypeRef, Constants.PROP_Order, Constants.TYPE_Double, ParseOrder);
                if (order.HasValue) return order.Value;
            }
            return 0;

            static double? ParseOrder(Instruction instruction)
            {
                return instruction.OpCode.Code == Code.Ldc_R8 ? (double)instruction.Operand : null;
            }
        }

        protected virtual Omit ExtractOmits()
        {
            if (MoTypeDef.Properties.Any(x => x.Name == Constants.PROP_MethodContextOmits && x.PropertyType.IsEnum(out var ptOmits) && ptOmits!.IsInt32()))
            {
                throw new FodyWeavingException($"[{MoTypeDef}] Since version 5.0.0, the MethodContextOmits property has been removed from the IMo interface. Use OptimizationAttribute instead. For more information, see https://github.com/inversionhourglass/Rougamo/releases/tag/v5.0.0");
            }
            var optimizationAttribute = MoTypeDef.CustomAttributes.FirstOrDefault(x => x.Is(Constants.TYPE_OptimizationAttribute));
            if (optimizationAttribute != null && optimizationAttribute.Properties.TryGet(Constants.PROP_MethodContext, out var property))
            {
                return (Omit)Convert.ToInt32(property!.Value.Argument.Value);
            }
            return Omit.None;
        }

        protected virtual ForceSync ExtractForceSync()
        {
            if (MoTypeDef.Properties.Any(x => x.Name == Constants.PROP_ForceSync && x.PropertyType.IsEnum(out var ptForceSync) && ptForceSync!.IsInt32()))
            {
                throw new FodyWeavingException($"[{MoTypeDef}] Since version 5.0.0, the ForceSync property has been removed from the IMo interface. Use OptimizationAttribute instead. For more information, see https://github.com/inversionhourglass/Rougamo/releases/tag/v5.0.0");
            }
            var optimizationAttribute = MoTypeDef.CustomAttributes.FirstOrDefault(x => x.Is(Constants.TYPE_OptimizationAttribute));
            if (optimizationAttribute != null && optimizationAttribute.Properties.TryGet(Constants.PROP_ForceSync, out var property))
            {
                return (ForceSync)Convert.ToInt32(property!.Value.Argument.Value);
            }
            return ForceSync.None;
        }

        protected virtual Lifetime ExtractLifetime()
        {
            if (IsStruct) return Lifetime.Transient;

            var lifetimeAttribute = MoTypeDef.CustomAttributes.FirstOrDefault(x => x.Is(Constants.TYPE_LifetimeAttribute));
            if (lifetimeAttribute != null && lifetimeAttribute.ConstructorArguments.Count == 1)
            {
                var arg = lifetimeAttribute.ConstructorArguments[0];
                return (Lifetime)Convert.ToInt32(arg.Value);
            }

            return Lifetime.Transient;
        }

        #region Extract-Property-Value

        private static T? ExtractFromIl<T>(TypeReference typeRef, string propertyName, string propertyTypeFullName, Func<Instruction, T?> tryResolve) where T : struct
        {
            return ExtractFromProp(typeRef, propertyName, tryResolve) ??
                ExtractFromCtor(typeRef, propertyTypeFullName, propertyName, tryResolve);
        }

        private static T? ExtractFromProp<T>(TypeReference typeRef, string propName, Func<Instruction, T?> tryResolve) where T : struct
        {
            var typeDef = typeRef.Resolve();
            while (typeDef != null)
            {
                var property = typeDef.Properties.FirstOrDefault(prop => prop.Name == propName);
                if (property != null)
                {
                    var instructions = property.GetMethod.Body.Instructions;
                    for (int i = instructions.Count - 1; i >= 0; i--)
                    {
                        var value = tryResolve(instructions[i]);
                        if (value.HasValue) return value.Value;
                    }
                    // 一旦在类定义中找到了属性定义，即使没有查找到对应初始化代码，也没有必要继续往父类查找了
                    // 因为已经override的属性，父类的赋值操作没有意义，直接进行后续的构造方法查找即可
                    return null;
                }
                typeDef = typeDef.BaseType?.Resolve();
            }
            return null;
        }

        private static T? ExtractFromCtor<T>(TypeReference typeRef, string propTypeFullName, string propertyName, Func<Instruction, T?> tryResolve) where T : struct
        {
            var propFieldName = string.Format(Constants.FIELD_Format, propertyName);
            var setterName = Constants.Setter(propertyName);
            var typeDef = typeRef.Resolve();
            while (typeDef != null)
            {
                var nonCtor = typeDef.GetConstructors().FirstOrDefault(ctor => !ctor.HasParameters);
                if (nonCtor != null)
                {
                    foreach (var instruction in nonCtor.Body.Instructions)
                    {
                        if (instruction.IsStfld(propFieldName, propTypeFullName) || instruction.IsCallAny(setterName))
                        {
                            return tryResolve(instruction.Previous);
                        }
                    }
                }
                typeDef = typeDef.BaseType?.Resolve();
            }
            return null;
        }

        private static T? ExtractFromIl<T>(TypeReference typeRef, string propertyName, string propertyTypeFullName, Func<Instruction, T?> tryResolve) where T : class
        {
            return ExtractFromProp(typeRef, propertyName, tryResolve) ??
                ExtractFromCtor(typeRef, propertyTypeFullName, propertyName, tryResolve);
        }

        private static T? ExtractFromProp<T>(TypeReference typeRef, string propName, Func<Instruction, T?> tryResolve) where T : class
        {
            var typeDef = typeRef.Resolve();
            while (typeDef != null)
            {
                var property = typeDef.Properties.FirstOrDefault(prop => prop.Name == propName);
                if (property != null)
                {
                    var instructions = property.GetMethod.Body.Instructions;
                    for (int i = instructions.Count - 1; i >= 0; i--)
                    {
                        var value = tryResolve(instructions[i]);
                        if (value != null) return value;
                    }
                    // 一旦在类定义中找到了属性定义，即使没有查找到对应初始化代码，也没有必要继续往父类查找了
                    // 因为已经override的属性，父类的赋值操作没有意义，直接进行后续的构造方法查找即可
                    return null;
                }
                typeDef = typeDef.BaseType?.Resolve();
            }
            return null;
        }

        private static T? ExtractFromCtor<T>(TypeReference typeRef, string propTypeFullName, string propertyName, Func<Instruction, T?> tryResolve) where T : class
        {
            var propFieldName = string.Format(Constants.FIELD_Format, propertyName);
            var setterName = Constants.Setter(propertyName);
            var typeDef = typeRef.Resolve();
            while (typeDef != null)
            {
                var nonCtor = typeDef.GetConstructors().FirstOrDefault(ctor => !ctor.HasParameters);
                if (nonCtor != null)
                {
                    foreach (var instruction in nonCtor.Body.Instructions)
                    {
                        if (instruction.IsStfld(propFieldName, propTypeFullName) || instruction.IsCallAny(setterName))
                        {
                            return tryResolve(instruction.Previous);
                        }
                    }
                }
                typeDef = typeDef.BaseType?.Resolve();
            }
            return null;
        }

        #endregion Extract-Property-Value

        class EqualityComparer : IEqualityComparer<Mo>
        {
            public bool Equals(Mo x, Mo y)
            {
                var argsX = ExtractTypeArgs(x);
                var argsY = ExtractTypeArgs(y);
                if (x.MoTypeRef.FullName != y.MoTypeRef.FullName || argsX.Count != argsY.Count) return false;
                for (var i = 0; i < argsX.Count; i++)
                {
                    if (!Equals(argsY[i], argsX[i])) return false;
                }
                var propsX = ExtractTypeProperties(x);
                var propsY = ExtractTypeProperties(y);
                if (propsX.Count != propsY.Count) return false;
                var propMapX = propsX.ToDictionary(x => x.Name, x => x.Argument);
                var propMapY = propsY.ToDictionary(x => x.Name, x => x.Argument);
                foreach (var xItem in propMapX)
                {
                    if (!propMapY.TryGetValue(xItem.Key, out var value)) return false;
                    if (!Equals(xItem.Value, value)) return false;
                }
                return true;
            }

            public int GetHashCode(Mo obj)
            {
                var args = ExtractTypeArgs(obj);
                var props = ExtractTypeProperties(obj);
                var hash = obj.MoTypeRef.GetHashCode();
                unchecked
                {
                    foreach (var arg in args)
                    {
                        hash = hash * 17 + GetHashCode(hash, arg);
                    }
                    foreach (var prop in props)
                    {
                        hash = hash * 17 + GetHashCode(hash, prop);
                    }
                    return hash;
                }
            }

            private int GetHashCode(int hash, CustomAttributeArgument arg)
            {
                if (arg.Value == null) return hash * 17 + 233;

                if (arg.Value is CustomAttributeArgument[] args)
                {
                    foreach (var a in args)
                    {
                        hash = hash * 17 + GetHashCode(hash, a);
                    }
                }
                else
                {
                    hash = hash * 17 + arg.Value.GetHashCode();
                }

                return hash;
            }

            private int GetHashCode(int hash, CustomAttributeNamedArgument arg)
            {
                hash = hash * 17 + arg.Name.GetHashCode();

                hash = hash * 17 + GetHashCode(hash, arg.Argument);

                return hash;
            }

            private Collection<CustomAttributeArgument> ExtractTypeArgs(Mo mo)
            {
                return mo is CustomAttributeMo caMo ? caMo.Attribute.ConstructorArguments : [];
            }

            private Collection<CustomAttributeNamedArgument> ExtractTypeProperties(Mo mo)
            {
                return mo is CustomAttributeMo caMo ? caMo.Attribute.Properties : [];
            }

            private bool Equals(CustomAttributeArgument arg1, CustomAttributeArgument arg2)
            {
                if (arg1.Value is CustomAttributeArgument[] args1 && arg2.Value is CustomAttributeArgument[] args2 && args1.Length == args2.Length)
                {
                    for (var i = 0; i < args1.Length; i++)
                    {
                        if (!Equals(args1[i], args2[i])) return false;
                    }
                    return true;
                }

                return Equals(arg1.Value, arg2.Value);
            }
        }
    }
}
