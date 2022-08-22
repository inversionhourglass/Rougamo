using Mono.Cecil;
using Mono.Collections.Generic;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    internal sealed class Mo
    {
        private AccessFlags? _flags;

        public Mo(CustomAttribute attribute, MoFrom from)
        {
            Attribute = attribute;
            From = from;
        }

        public Mo(TypeDefinition typeDef, MoFrom from)
        {
            TypeDef = typeDef;
            From = from;
        }

        public static IEqualityComparer<Mo> Comparer { get; } = new EqualityComparer();

        public CustomAttribute? Attribute { get; }

        public TypeDefinition? TypeDef { get; }

        public MoFrom From { get; }

        public AccessFlags Flags
        {
            get
            {
                if (!_flags.HasValue)
                {
                    _flags = Attribute != null ? this.ExtractFlagsFromAttribute() : this.ExtractFlagsFromType();
                }
                return _flags.Value;
            }
        }

        public string FullName => Attribute?.AttributeType?.FullName ?? TypeDef!.FullName;

        class EqualityComparer : IEqualityComparer<Mo>
        {
            public bool Equals(Mo x, Mo y)
            {
                (var typeX, var argsX) = ExtractTypeArgs(x);
                (var typeY, var argsY) = ExtractTypeArgs(y);
                if (typeX.FullName != typeY.FullName || argsX.Count != argsY.Count) return false;
                for (var i = 0; i < argsX.Count; i++)
                {
                    if(!Equals(argsY[i], argsX[i])) return false;
                }
                return true;
            }

            public int GetHashCode(Mo obj)
            {
                (var type, var args) = ExtractTypeArgs(obj);
                var hash = type.GetHashCode();
                unchecked
                {
                    foreach (var arg in args)
                    {
                        hash = hash * 17 + GetHashCode(hash, arg);
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

            private (TypeDefinition, Collection<CustomAttributeArgument>) ExtractTypeArgs(Mo mo)
            {
                if (mo.Attribute != null)
                {
                    return (mo.Attribute.AttributeType.Resolve(), mo.Attribute.ConstructorArguments);
                }
                return (mo.TypeDef!, new Collection<CustomAttributeArgument>());
            }

            private bool Equals(CustomAttributeArgument arg1, CustomAttributeArgument arg2)
            {
                if(arg1.Value is CustomAttributeArgument[] args1 && arg2.Value is CustomAttributeArgument[] args2 && args1.Length == args2.Length)
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
