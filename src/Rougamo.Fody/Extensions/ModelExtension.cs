using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Cecil.AspectN;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    internal static class ModelExtension
    {
        #region Mo

        #region Extract-Mo-Flags

        public static AccessFlags ExtractFlags(this Mo mo)
        {
            var typeRef = mo.TypeRef;
            if (mo.Attribute != null)
            {
                if (mo.Attribute.AttributeType.Implement(Constants.TYPE_IFlexibleModifierPointcut) && mo.Attribute.Properties.TryGet(Constants.PROP_Flags, out var property))
                {
                    return (AccessFlags)Convert.ToInt32(property!.Value.Argument.Value);
                }
                typeRef = mo.Attribute.AttributeType;
            }
            var typeDef = typeRef!.ToDefinition();
            var implementedInterface = typeDef.Implement(Constants.TYPE_IFlexibleModifierPointcut);
            if (!implementedInterface && typeDef.Properties.Any(x => x.Name == Constants.PROP_Flags && x.PropertyType.IsEnum(out var ptFlag) && ptFlag!.IsInt32()))
            {
                throw new FodyWeavingException($"[{typeDef}] Since version 5.0.0, the Flags property has been removed from the IMo interface. Use PointcutAttribute instead. For more information, see https://github.com/inversionhourglass/Rougamo/releases/tag/v5.0.0");
            }
            var pointcutAttribute = typeDef.CustomAttributes.FirstOrDefault(x => x.Is(Constants.TYPE_PointcutAttribute));
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
                flags = ExtractFromIl(typeRef!, Constants.PROP_Flags, Constants.TYPE_AccessFlags, ParseFlags);
            }
            return flags ?? AccessFlags.InstancePublic;
        }

        private static AccessFlags? ParseFlags(Instruction instruction)
        {
            var opCode = instruction.OpCode;
            if (opCode == OpCodes.Ldc_I4_3) return AccessFlags.Public;
            if (opCode == OpCodes.Ldc_I4_6) return AccessFlags.Instance;
            if (opCode == OpCodes.Ldc_I4_7) return AccessFlags.Instance | AccessFlags.Public;
            if (opCode == OpCodes.Ldc_I4_S || opCode == OpCodes.Ldc_I4) return (AccessFlags)Convert.ToInt32(instruction.Operand);
            return null;
        }

        #endregion Extract-Mo-Flags

        #region Extract-Mo-Pattern

        public static string? ExtractPattern(this Mo mo)
        {
            var typeRef = mo.TypeRef;
            if (mo.Attribute != null)
            {
                if (mo.Attribute.AttributeType.Implement(Constants.TYPE_IFlexiblePatternPointcut) && mo.Attribute.Properties.TryGet(Constants.PROP_Pattern, out var property))
                {
                    return (string)property!.Value.Argument.Value;
                }
                typeRef = mo.Attribute.AttributeType;
            }
            var typeDef = typeRef!.ToDefinition();
            var implementedInterface = typeDef.Implement(Constants.TYPE_IFlexiblePatternPointcut);
            if (!implementedInterface && typeDef.Properties.Any(x => x.Name == Constants.PROP_Pattern && x.PropertyType.IsString()))
            {
                throw new FodyWeavingException($"[{typeDef}] Since version 5.0.0, the Pattern property has been removed from the IMo interface. Use PointcutAttribute instead. For more information, see https://github.com/inversionhourglass/Rougamo/releases/tag/v5.0.0");
            }
            var pointcutAttribute = typeDef.CustomAttributes.FirstOrDefault(x => x.Is(Constants.TYPE_PointcutAttribute));
            if (pointcutAttribute != null && pointcutAttribute.ConstructorArguments.Count == 1)
            {
                var arg = pointcutAttribute.ConstructorArguments[0];
                if (arg.Type.IsString())
                {
                    return (string)arg.Value;
                }
            }
            return implementedInterface ? ExtractFromIl(typeRef!, Constants.PROP_Pattern, Constants.TYPE_String, ParsePattern) : null;
        }

        private static string? ParsePattern(Instruction instruction)
        {
            return instruction.OpCode.Code == Code.Ldstr ? (string)instruction.Operand : null;
        }

        #endregion Extract-Mo-Pattern

        #region Extract-Mo-Features

        public static Feature ExtractFeatures(this Mo mo)
        {
            var typeRef = mo.Attribute == null ? mo.TypeRef : mo.Attribute.AttributeType;
            var typeDef = typeRef!.ToDefinition();
            if (typeDef.Properties.Any(x => x.Name == Constants.PROP_Features && x.PropertyType.IsEnum(out var ptFeature) && ptFeature!.IsInt32()))
            {
                throw new FodyWeavingException($"[{typeDef}] Since version 5.0.0, the Features property has been removed from the IMo interface. Use AdviceAttribute instead. For more information, see https://github.com/inversionhourglass/Rougamo/releases/tag/v5.0.0");
            }
            var adviceAttribute = typeDef.CustomAttributes.FirstOrDefault(x => x.Is(Constants.TYPE_AdviceAttribute));
            if (adviceAttribute != null && adviceAttribute.ConstructorArguments.Count == 1)
            {
                var arg = adviceAttribute.ConstructorArguments[0];
                return (Feature)Convert.ToInt32(arg.Value);
            }
            return Feature.All;
        }

        #endregion Extract-Mo-Features

        #region Extract-Mo-Order

        public static double ExtractOrder(this Mo mo)
        {
            var typeDef = mo.TypeRef;
            if (mo.Attribute != null)
            {
                if (mo.Attribute.Properties.TryGet(Constants.PROP_Order, out var property))
                {
                    return (double)property!.Value.Argument.Value;
                }
                typeDef = mo.Attribute.AttributeType.Resolve();
            }
            var order = ExtractFromIl(typeDef!, Constants.PROP_Order, Constants.TYPE_Double, ParseOrder);
            return order ?? 0;
        }

        private static double? ParseOrder(Instruction instruction)
        {
            return instruction.OpCode.Code == Code.Ldc_R8 ? (double)instruction.Operand : null;
        }

        #endregion Extract-Mo-Order

        #region Extract-Mo-MethodContextOmits

        public static Omit ExtractOmits(this Mo mo)
        {
            var typeRef = mo.Attribute == null ? mo.TypeRef : mo.Attribute.AttributeType;
            var typeDef = typeRef!.ToDefinition();
            if (typeDef.Properties.Any(x => x.Name == Constants.PROP_MethodContextOmits && x.PropertyType.IsEnum(out var ptOmits) && ptOmits!.IsInt32()))
            {
                throw new FodyWeavingException($"[{typeDef}] Since version 5.0.0, the MethodContextOmits property has been removed from the IMo interface. Use OptimizationAttribute instead. For more information, see https://github.com/inversionhourglass/Rougamo/releases/tag/v5.0.0");
            }
            var optimizationAttribute = typeDef.CustomAttributes.FirstOrDefault(x => x.Is(Constants.TYPE_OptimizationAttribute));
            if (optimizationAttribute != null && optimizationAttribute.Properties.TryGet(Constants.PROP_MethodContext, out var property))
            {
                return (Omit)Convert.ToInt32(property!.Value.Argument.Value);
            }
            return Omit.None;
        }

        #endregion Extract-Mo-MethodContextOmits

        #region Extract-Mo-ForceSync

        public static ForceSync ExtractForceSync(this Mo mo)
        {
            var typeRef = mo.Attribute == null ? mo.TypeRef : mo.Attribute.AttributeType;
            var typeDef = typeRef!.ToDefinition();
            if (typeDef.Properties.Any(x => x.Name == Constants.PROP_ForceSync && x.PropertyType.IsEnum(out var ptForceSync) && ptForceSync!.IsInt32()))
            {
                throw new FodyWeavingException($"[{typeDef}] Since version 5.0.0, the ForceSync property has been removed from the IMo interface. Use OptimizationAttribute instead. For more information, see https://github.com/inversionhourglass/Rougamo/releases/tag/v5.0.0");
            }
            var optimizationAttribute = typeDef.CustomAttributes.FirstOrDefault(x => x.Is(Constants.TYPE_OptimizationAttribute));
            if (optimizationAttribute != null && optimizationAttribute.Properties.TryGet(Constants.PROP_ForceSync, out var property))
            {
                return (ForceSync)Convert.ToInt32(property!.Value.Argument.Value);
            }
            return ForceSync.None;
        }

        #endregion Extract-Mo-ForceSync

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

        public static void Initialize(this RouType rouType, MethodDefinition methdDef,
            CustomAttribute[] assemblyAttributes, TypeReference[] assemblyGenerics,
            RepulsionMo[] typeImplements, CustomAttribute[] typeAttributes, TypeReference[] typeGenerics, TypeReference[] typeProxies,
            CustomAttribute[] methodAttributes, TypeReference[] methodGenerics, TypeReference[] methodProxies,
            string[] assemblyIgnores, string[] typeIgnores, string[] methodIgnores, bool compositeAccessibility, bool skipRefStruct)
        {
            var ignores = new HashSet<string>(assemblyIgnores);
            ignores.AddRange(typeIgnores);
            ignores.AddRange(methodIgnores);

            var rouMethod = new RouMethod(rouType, methdDef, skipRefStruct);

            rouMethod.AddMo(methodAttributes.Where(x => !ignores.Contains(x.AttributeType.FullName)), MoFrom.Method, compositeAccessibility);
            rouMethod.AddMo(methodGenerics.Where(x => !ignores.Contains(x.FullName)), MoFrom.Method, compositeAccessibility);
            rouMethod.AddMo(methodProxies.Where(x => !ignores.Contains(x.FullName)), MoFrom.Method, compositeAccessibility);

            rouMethod.AddMo(typeAttributes.Where(x => !ignores.Contains(x.AttributeType.FullName)), MoFrom.Class, compositeAccessibility);
            rouMethod.AddMo(typeGenerics.Where(x => !ignores.Contains(x.FullName)), MoFrom.Class, compositeAccessibility);
            rouMethod.AddMo(typeProxies.Where(x => !ignores.Contains(x.FullName)), MoFrom.Class, compositeAccessibility);
            foreach (var implement in typeImplements.Where(x => !ignores.Contains(x.Mo.FullName)))
            {
                if (!rouMethod.Any(implement.Repulsions))
                {
                    rouMethod.AddMo(implement.Mo, MoFrom.Class, compositeAccessibility);
                    ignores.AddRange(implement.Repulsions.Select(x => x.FullName));
                }
            }

            rouMethod.AddMo(assemblyAttributes.Where(x => !ignores.Contains(x.AttributeType.FullName)), MoFrom.Assembly, compositeAccessibility);
            rouMethod.AddMo(assemblyGenerics.Where(x => !ignores.Contains(x.FullName)), MoFrom.Assembly, compositeAccessibility);

            if (rouMethod.MosAny())
            {
                rouType.Methods.Add(rouMethod);
            }
        }

        public static void AddMo(this RouMethod method, TypeReference typeRef, MoFrom from, bool compositeAccessibility) => AddMo(method, new[] { typeRef }, from, compositeAccessibility);

        public static void AddMo(this RouMethod method, IEnumerable<CustomAttribute> attributes, MoFrom from, bool compositeAccessibility)
        {
            var mos = attributes.Select(x => new Mo(x, from)).Where(x => MatchMo(method, x, from, compositeAccessibility));
            method.AddMo(mos);
        }

        public static void AddMo(this RouMethod method, IEnumerable<TypeReference> typeRefs, MoFrom from, bool compositeAccessibility)
        {
            var mos = typeRefs.Select(x => new Mo(x, from)).Where(x => MatchMo(method, x, from, compositeAccessibility));
            method.AddMo(mos);
        }

        private static bool MatchMo(RouMethod method, Mo mo, MoFrom from, bool compositeAccessibility)
        {
            if (from == MoFrom.Method) return true;

            return mo.Pattern == null ? MatchMoFlags(method, mo, from) : MatchMoPattern(method, mo, from, compositeAccessibility);
        }

        private static bool MatchMoFlags(RouMethod method, Mo mo, MoFrom from)
        {
            var methodFlags = method.Flags(from);
            var targetAccessable = methodFlags & AccessFlags.All;
            var targetCategory = methodFlags & (AccessFlags.Method | AccessFlags.Property | AccessFlags.Constructor);
            var declaredAccessable = mo.Flags & AccessFlags.All;
            var declaredCategories = mo.Flags & (AccessFlags.Method | AccessFlags.Property | AccessFlags.Constructor);
            if (declaredCategories == 0) declaredCategories = AccessFlags.Method | AccessFlags.Property;
            var categoryMatch = (declaredCategories & targetCategory) != 0;
            var accessableMatch = (declaredAccessable & targetAccessable) != 0;
            return categoryMatch && accessableMatch;
        }

        private static bool MatchMoPattern(RouMethod method, Mo mo, MoFrom from, bool compositeAccessibility)
        {
            compositeAccessibility = from == MoFrom.Assembly && compositeAccessibility;
            var signature = SignatureParser.ParseMethod(method.MethodDef, compositeAccessibility);
            var pattern = PatternParser.Parse(mo.Pattern!);
            return pattern.IsMatch(signature);
        }

        public static bool Any(this RouMethod method, TypeReference typeRef)
        {
            return method.MosAny(mo => mo.FullName == typeRef.FullName);
        }

        public static bool Any(this RouMethod method, TypeReference[] typeRefs)
        {
            return typeRefs.Any(method.Any);
        }

        #endregion Mo
    }
}
