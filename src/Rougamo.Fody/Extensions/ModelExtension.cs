using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Rougamo.Fody.Signature;
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
                if (mo.Attribute.Properties.TryGet(Constants.PROP_Flags, out var property))
                {
                    return (AccessFlags)Convert.ToInt32(property!.Value.Argument.Value);
                }
                typeRef = mo.Attribute.AttributeType;
            }
            var flags = ExtractFromIl(typeRef!, Constants.PROP_Flags, Constants.TYPE_AccessFlags, ParseFlags);
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
                if (mo.Attribute.Properties.TryGet(Constants.PROP_Pattern, out var property))
                {
                    return (string)property!.Value.Argument.Value;
                }
                typeRef = mo.Attribute.AttributeType;
            }
            return ExtractFromIl(typeRef!, Constants.PROP_Pattern, Constants.TYPE_String, ParsePattern);
        }

        private static string? ParsePattern(Instruction instruction)
        {
            return instruction.OpCode.Code == Code.Ldstr ? (string)instruction.Operand : null;
        }

        #endregion Extract-Mo-Pattern

        #region Extract-Mo-Features

        public static int ExtractFeatures(this Mo mo)
        {
            var typeDef = mo.TypeRef;
            if (mo.Attribute != null)
            {
                if (mo.Attribute.Properties.TryGet(Constants.PROP_Features, out var property))
                {
                    return Convert.ToInt32(property!.Value.Argument.Value);
                }
                typeDef = mo.Attribute.AttributeType.Resolve();
            }
            var features = ExtractFromIl(typeDef!, Constants.PROP_Features, Constants.TYPE_Feature, ParseFeatures);
            return features ?? (int)Feature.All;
        }

        private static int? ParseFeatures(Instruction instruction) => instruction.TryResolveInt32();

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
            var typeDef = mo.TypeRef;
            if (mo.Attribute != null)
            {
                if (mo.Attribute.Properties.TryGet(Constants.PROP_MethodContextOmits, out var property))
                {
                    return (Omit)Convert.ToInt32(property!.Value.Argument.Value);
                }
                typeDef = mo.Attribute.AttributeType.Resolve();
            }
            var flags = ExtractFromIl(typeDef!, Constants.PROP_MethodContextOmits, Constants.TYPE_Omit, ParseOmits);
            return flags ?? Omit.None;
        }

        private static Omit? ParseOmits(Instruction instruction)
        {
            var opCode = instruction.OpCode;
            if (opCode == OpCodes.Ldc_I4_0) return Omit.None;
            if (opCode == OpCodes.Ldc_I4_1) return Omit.Mos;
            if (opCode == OpCodes.Ldc_I4_2) return Omit.Arguments;
            if (opCode == OpCodes.Ldc_I4_3) return Omit.Mos | Omit.Arguments;
            if (opCode == OpCodes.Ldc_I4_4) return Omit.ReturnValue;
            if (opCode == OpCodes.Ldc_I4_5) return Omit.Mos | Omit.ReturnValue;
            if (opCode == OpCodes.Ldc_I4_6) return Omit.Arguments | Omit.ReturnValue;
            if (opCode == OpCodes.Ldc_I4_7) return Omit.All;
            return null;
        }

        #endregion Extract-Mo-MethodContextOmits

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

        public static bool IsMatch(this Feature matchWith, int value)
        {
            return ((int)matchWith & value) == (int)matchWith;
        }

        public static void Initialize(this RouType rouType, MethodDefinition methdDef,
            CustomAttribute[] assemblyAttributes, TypeReference[] assemblyGenerics,
            RepulsionMo[] typeImplements, CustomAttribute[] typeAttributes, TypeReference[] typeGenerics, TypeReference[] typeProxies,
            CustomAttribute[] methodAttributes, TypeReference[] methodGenerics, TypeReference[] methodProxies,
            string[] assemblyIgnores, string[] typeIgnores, string[] methodIgnores, bool compositeAccessibility)
        {
            var ignores = new HashSet<string>(assemblyIgnores);
            ignores.AddRange(typeIgnores);
            ignores.AddRange(methodIgnores);

            var rouMethod = new RouMethod(rouType, methdDef);

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
