using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
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
            var typeDef = mo.TypeDef;
            if (mo.Attribute != null)
            {
                if (mo.Attribute.Properties.TryGet(Constants.PROP_Flags, out var property))
                {
                    return (AccessFlags)(sbyte)property!.Value.Argument.Value;
                }
                typeDef = mo.Attribute.AttributeType.Resolve();
            }
            var flags = ExtractFromIl(typeDef!, Constants.PROP_Flags, Constants.TYPE_AccessFlags, ParseFlags);
            return flags ?? AccessFlags.InstancePublic;
        }

        private static AccessFlags? ParseFlags(Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Ldc_I4_3) return AccessFlags.Public;
            if (instruction.OpCode == OpCodes.Ldc_I4_6) return AccessFlags.Instance;
            if (instruction.OpCode == OpCodes.Ldc_I4_7) return AccessFlags.Instance | AccessFlags.Public;
            if (instruction.OpCode == OpCodes.Ldc_I4_S) return (AccessFlags)(sbyte)instruction.Operand;
            return null;
        }

        #endregion Extract-Mo-Flags

        #region Extract-Mo-Features

        public static int ExtractFeatures(this Mo mo)
        {
            var typeDef = mo.TypeDef;
            if (mo.Attribute != null)
            {
                if (mo.Attribute.Properties.TryGet(Constants.PROP_Features, out var property))
                {
                    return (int)property!.Value.Argument.Value;
                }
                typeDef = mo.Attribute.AttributeType.Resolve();
            }
            var features = ExtractFromIl(typeDef!, Constants.PROP_Features, Constants.TYPE_Int32, ParseFeatures);
            return features ?? (int)Feature.All;
        }

        private static int? ParseFeatures(Instruction instruction) => instruction.TryResolveInt32();

        #endregion Extract-Mo-Features

        #region Extract-Mo-Order

        public static double ExtractOrder(this Mo mo)
        {
            var typeDef = mo.TypeDef;
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

        #region Extract-Property-Value

        private static T? ExtractFromIl<T>(TypeDefinition typeDef, string propertyName, string propertyTypeFullName, Func<Instruction, T?> tryResolve) where T : struct
        {
            return ExtractFromProp(typeDef, propertyName, tryResolve) ??
                ExtractFromCtor(typeDef, propertyTypeFullName, string.Format(Constants.FIELD_Format, propertyName), tryResolve);
        }

        private static T? ExtractFromProp<T>(TypeDefinition typeDef, string propName, Func<Instruction, T?> tryResolve) where T : struct
        {
            do
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
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                typeDef = typeDef.BaseType?.Resolve();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            } while (typeDef != null);
            return null;
        }

        private static T? ExtractFromCtor<T>(TypeDefinition typeDef, string propTypeFullName, string propFieldName, Func<Instruction, T?> tryResolve) where T : struct
        {
            do
            {
                var nonCtor = typeDef.GetConstructors().FirstOrDefault(ctor => !ctor.HasParameters);
                if (nonCtor != null)
                {
                    foreach (var instruction in nonCtor.Body.Instructions)
                    {
                        if (instruction.IsStfld(propFieldName, propTypeFullName))
                        {
                            return tryResolve(instruction.Previous);
                        }
                    }
                }
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                typeDef = typeDef.BaseType?.Resolve();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            } while (typeDef != null);
            return null;
        }

        #endregion Extract-Property-Value

        public static bool IsMatch(this Feature matchWith, int value)
        {
            return ((int)matchWith & value) == (int)matchWith;
        }

        public static void Initialize(this RouType rouType, MethodDefinition methdDef, CustomAttribute[] assemblyAttributes,
            RepulsionMo[] typeImplements, CustomAttribute[] typeAttributes, TypeDefinition[] typeProxies,
            CustomAttribute[] methodAttributes, TypeDefinition[] methodProxies,
            string[] assemblyIgnores, string[] typeIgnores, string[] methodIgnores)
        {
            var ignores = new HashSet<string>(assemblyIgnores);
            ignores.AddRange(typeIgnores);
            ignores.AddRange(methodIgnores);

            var rouMethod = new RouMethod(methdDef);

            rouMethod.AddMo(methodAttributes.Where(x => !ignores.Contains(x.AttributeType.FullName)), MoFrom.Method);
            rouMethod.AddMo(methodProxies.Where(x => !ignores.Contains(x.FullName)), MoFrom.Method);

            rouMethod.AddMo(typeAttributes.Where(x => !ignores.Contains(x.AttributeType.FullName)), MoFrom.Class);
            rouMethod.AddMo(typeProxies.Where(x => !ignores.Contains(x.FullName)), MoFrom.Class);
            foreach (var implement in typeImplements.Where(x => !ignores.Contains(x.Mo.FullName)))
            {
                if (!rouMethod.Any(implement.Repulsions))
                {
                    rouMethod.AddMo(implement.Mo, MoFrom.Class);
                    ignores.AddRange(implement.Repulsions.Select(x => x.FullName));
                }
            }

            rouMethod.AddMo(assemblyAttributes.Where(x => !ignores.Contains(x.AttributeType.FullName)), MoFrom.Assembly);

            if (rouMethod.MosAny())
            {
                rouType.Methods.Add(rouMethod);
            }
        }

        public static void AddMo(this RouMethod method, TypeDefinition typeDef, MoFrom from)
        {
            var mo = new Mo(typeDef, from);
            if((method.Flags(from) & mo.Flags) != 0)
            {
                method.AddMo(mo);
            }
        }

        public static void AddMo(this RouMethod method, IEnumerable<CustomAttribute> attributes, MoFrom from)
        {
            var mos = attributes.Select(x => new Mo(x, from)).Where(x => (x.Flags & method.Flags(from)) != 0);
            method.AddMo(mos);
        }

        public static void AddMo(this RouMethod method, IEnumerable<TypeDefinition> typeDefs, MoFrom from)
        {
            var mos = typeDefs.Select(x => new Mo(x, from)).Where(x => (x.Flags & method.Flags(from)) != 0);
            method.AddMo(mos);
        }

        public static bool Any(this RouMethod method, TypeDefinition typeDef)
        {
            return method.MosAny(mo => mo.FullName == typeDef.FullName);
        }

        public static bool Any(this RouMethod method, TypeDefinition[] typeDefs)
        {
            return typeDefs.Any(method.Any);
        }

        #endregion Mo
    }
}
