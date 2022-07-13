using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    internal static class ModelExtension
    {
        #region Mo

        #region Mo-Flags

        public static AccessFlags ExtractFlagsFromAttribute(this Mo mo)
        {
            if (mo.Attribute!.Properties.TryGet(Constants.PROP_Flags, out var property))
            {
                return (AccessFlags)(sbyte)property!.Value.Argument.Value;
            }
            var flags = ExtractFlagsFromIl(mo.Attribute.AttributeType.Resolve());
            return flags.HasValue ? flags.Value : AccessFlags.InstancePublic;
        }

        public static AccessFlags ExtractFlagsFromType(this Mo mo)
        {
            var flags = ExtractFlagsFromIl(mo.TypeDef!);
            return flags.HasValue ? flags.Value : AccessFlags.InstancePublic;
        }

        private static AccessFlags? ExtractFlagsFromIl(TypeDefinition typeDef)
        {
            return ExtractFlagsFromProp(typeDef) ?? ExtractFlagsFromCtor(typeDef);
        }

        private static AccessFlags? ExtractFlagsFromProp(TypeDefinition typeDef)
        {
            do
            {
                var property = typeDef.Properties.FirstOrDefault(prop => prop.Name == Constants.PROP_Flags);
                if (property != null)
                {
                    var instructions = property.GetMethod.Body.Instructions;
                    for (int i = instructions.Count - 1; i >= 0; i--)
                    {
                        var flags = ParseFlags(instructions[i]);
                        if (flags.HasValue) return flags.Value;
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

        private static AccessFlags? ExtractFlagsFromCtor(TypeDefinition typeDef)
        {
            do
            {
                var nonCtor = typeDef.GetConstructors().FirstOrDefault(ctor => !ctor.HasParameters);
                if (nonCtor != null)
                {
                    foreach (var instruction in nonCtor.Body.Instructions)
                    {
                        if (instruction.IsStfld(Constants.FIELD_Flags, Constants.TYPE_AccessFlags))
                        {
                            return ParseFlags(instruction.Previous);
                        }
                    }
                }
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                typeDef = typeDef.BaseType?.Resolve();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            } while (typeDef != null);
            return null;
        }

        private static AccessFlags? ParseFlags(Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Ldc_I4_3) return AccessFlags.Public;
            if (instruction.OpCode == OpCodes.Ldc_I4_6) return AccessFlags.Instance;
            if (instruction.OpCode == OpCodes.Ldc_I4_7) return AccessFlags.Instance | AccessFlags.Public;
            if (instruction.OpCode == OpCodes.Ldc_I4_S) return (AccessFlags)(sbyte)instruction.Operand;
            return null;
        }

        #endregion Mo-Flags

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

            if (rouMethod.Mos.Any())
            {
                rouType.Methods.Add(rouMethod);
            }
        }

        public static void AddMo(this RouMethod method, TypeDefinition typeDef, MoFrom from)
        {
            var mo = new Mo(typeDef, from);
            if((method.Flags(from) & mo.Flags) != 0)
            {
                method.Mos.Add(mo);
            }
        }

        public static void AddMo(this RouMethod method, IEnumerable<CustomAttribute> attributes, MoFrom from)
        {
            var mos = attributes.Select(x => new Mo(x, from)).Where(x => (x.Flags & method.Flags(from)) != 0);
            method.Mos.AddRange(mos);
        }

        public static void AddMo(this RouMethod method, IEnumerable<TypeDefinition> typeDefs, MoFrom from)
        {
            var mos = typeDefs.Select(x => new Mo(x, from)).Where(x => (x.Flags & method.Flags(from)) != 0);
            method.Mos.AddRange(mos);
        }

        public static bool Any(this RouMethod method, TypeDefinition typeDef)
        {
            return method.Mos.Any(mo => mo.FullName == typeDef.FullName);
        }

        public static bool Any(this RouMethod method, TypeDefinition[] typeDefs)
        {
            return typeDefs.Any(typeDef => method.Any(typeDef));
        }

        #endregion Mo

        public static ReturnType ToReturnType(this TypeReference returnTypeRef)
        {
            if (returnTypeRef.IsVoid()) return ReturnType.Void;

            return returnTypeRef.IsValueType || returnTypeRef.IsEnum(out _) && !returnTypeRef.IsArray || returnTypeRef.IsGenericParameter ? ReturnType.ValueType : ReturnType.ReferenceType;
        }
    }
}
