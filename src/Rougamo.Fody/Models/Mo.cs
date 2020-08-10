using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Linq;

namespace Rougamo.Fody
{
    internal sealed class Mo
    {
        private AccessFlags? _flags;

        public Mo(CustomAttribute attribute)
        {
            Attribute = attribute;
        }

        public Mo(TypeDefinition typeDef)
        {
            TypeDef = TypeDef;
        }

        public CustomAttribute Attribute { get; set; }

        public TypeDefinition TypeDef { get; set; }

        public AccessFlags Flags
        {
            get
            {
                if (!_flags.HasValue)
                {
                    _flags = Attribute != null ? ExtractFlagsFromAttribute() : ExtractFlagsFromType();
                }
                return _flags.Value;
            }
        }

        private AccessFlags ExtractFlagsFromAttribute()
        {
            if(Attribute.Properties.TryGet(Constants.PROP_Flags, out var property))
            {
                return (AccessFlags)property.Value.Argument.Value;
            }
            var flags = ExtractFlagsFromIl(Attribute.AttributeType.Resolve());
            return flags.HasValue ? flags.Value : AccessFlags.InstancePublic;
        }

        private AccessFlags ExtractFlagsFromType()
        {
            var flags = ExtractFlagsFromIl(TypeDef);
            return flags.HasValue ? flags.Value : AccessFlags.InstancePublic;
        }

        private AccessFlags? ExtractFlagsFromIl(TypeDefinition typeDef)
        {
            return ExtractFlagsFromProp(typeDef) ?? ExtractFlagsFromCtor(typeDef);
        }

        private AccessFlags? ExtractFlagsFromProp(TypeDefinition typeDef)
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
                typeDef = typeDef.BaseType?.Resolve();
            } while (typeDef != null);
            return null;
        }

        private AccessFlags? ExtractFlagsFromCtor(TypeDefinition typeDef)
        {
            do
            {
                var nonCtor = typeDef.GetConstructors().FirstOrDefault(ctor => !ctor.HasParameters);
                if(nonCtor != null)
                {
                    foreach(var instruction in nonCtor.Body.Instructions)
                    {
                        if(instruction.IsStfld(Constants.FIELD_Flags, Constants.TYPE_AccessFlags))
                        {
                            return ParseFlags(instruction.Previous).Value;
                        }
                    }
                }
                typeDef = typeDef.BaseType?.Resolve();
            } while (typeDef != null);
            return null;
        }

        private AccessFlags? ParseFlags(Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Ldc_I4_3) return AccessFlags.Public;
            if (instruction.OpCode == OpCodes.Ldc_I4_6) return AccessFlags.Instance;
            if (instruction.OpCode == OpCodes.Ldc_I4_7) return AccessFlags.Instance | AccessFlags.Public;
            if (instruction.OpCode == OpCodes.Ldc_I4_S) return (AccessFlags)instruction.Operand;
            return null;
        }
    }
}
