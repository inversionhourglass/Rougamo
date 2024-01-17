using Mono.Cecil;
using Mono.Collections.Generic;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    internal sealed class Mo
    {
        private AccessFlags? _flags;
        private string? _pattern;
        private bool _patternSet;
        private int? _features;
        private double? _order;

        public Mo(CustomAttribute attribute, MoFrom from)
        {
            Attribute = attribute;
            From = from;
            IsStruct = false;
        }

        public Mo(TypeDefinition typeDef, MoFrom from)
        {
            TypeDef = typeDef;
            From = from;
            IsStruct = TypeDef.IsValueType;
        }

        public static IEqualityComparer<Mo> Comparer { get; } = new EqualityComparer();

        public CustomAttribute? Attribute { get; }

        public TypeDefinition? TypeDef { get; }

        public MoFrom From { get; }

        public bool IsStruct { get; }

        public AccessFlags Flags
        {
            get
            {
                if (!_flags.HasValue)
                {
                    _flags = this.ExtractFlags();
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
                    _pattern = this.ExtractPattern();
                    _patternSet = true;
                }
                return _pattern;
            }
        }

        public int Features
        {
            get
            {
                if (!_features.HasValue)
                {
                    _features = this.ExtractFeatures();
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
                    _order = this.ExtractOrder();
                }
                return _order.Value;
            }
        }

        public MethodReference On(string methodName, ModuleDefinition moduleDef)
        {
            var typeDef = TypeDef ?? Attribute!.AttributeType.Resolve();
            return typeDef.Methods.FirstOrDefault(x => x.Name == methodName).ImportInto(moduleDef);
        }

        public string FullName => Attribute?.AttributeType?.FullName ?? TypeDef!.FullName;

        class EqualityComparer : IEqualityComparer<Mo>
        {
            public bool Equals(Mo x, Mo y)
            {
                var argsX = ExtractTypeArgs(x, out var typeX);
                var argsY = ExtractTypeArgs(y, out var typeY);
                if (typeX.FullName != typeY.FullName || argsX.Count != argsY.Count) return false;
                for (var i = 0; i < argsX.Count; i++)
                {
                    if(!Equals(argsY[i], argsX[i])) return false;
                }
                return true;
            }

            public int GetHashCode(Mo obj)
            {
                var args = ExtractTypeArgs(obj, out var type);
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

            private Collection<CustomAttributeArgument> ExtractTypeArgs(Mo mo, out TypeDefinition typeDef)
            {
                if (mo.Attribute != null)
                {
                    typeDef = mo.Attribute.AttributeType.Resolve();
                    return mo.Attribute.ConstructorArguments;
                }
                typeDef = mo.TypeDef!;
                return new Collection<CustomAttributeArgument>();
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
