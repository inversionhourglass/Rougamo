using Mono.Cecil;
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
                return x.FullName == y.FullName;
            }

            public int GetHashCode(Mo obj)
            {
                return obj.FullName.GetHashCode();
            }
        }
    }
}
