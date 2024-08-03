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
        private Feature? _features;
        private double? _order;
        private Omit? _omit;
        private ForceSync? _forceSync;

        public Mo(CustomAttribute attribute, MoFrom from)
        {
            Attribute = attribute;
            MoTypeDef = attribute.AttributeType.Resolve();
            From = from;
            IsStruct = false;
        }

        public Mo(TypeReference typeRef, MoFrom from)
        {
            TypeRef = typeRef;
            MoTypeDef = typeRef.Resolve();
            From = from;
            IsStruct = MoTypeDef.IsValueType;
        }

        public static IEqualityComparer<Mo> Comparer { get; } = new EqualityComparer();

        public CustomAttribute? Attribute { get; }

        public TypeReference? TypeRef { get; }

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

        public Feature Features
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

        public Omit MethodContextOmits
        {
            get
            {
                if (!_omit.HasValue)
                {
                    _omit = this.ExtractOmits();
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
                    _forceSync = this.ExtractForceSync();
                }
                return _forceSync.Value;
            }
        }

        public TypeDefinition MoTypeDef { get; }

        public TypeReference MoTypeRef => TypeRef ?? Attribute!.AttributeType;

        public MethodReference? On(string methodName, ModuleDefinition moduleDef)
        {
            var methodRef = MoTypeDef.Methods.FirstOrDefault(x => x.Name == methodName)?.ImportInto(moduleDef);
            if (methodRef == null) return null;

            var moTypeRef = MoTypeRef.ImportInto(moduleDef);
            if (moTypeRef is GenericInstanceType git)
            {
                methodRef = methodRef.WithGenericDeclaringType(git);
            }
            return methodRef;
        }

        public string FullName => Attribute?.AttributeType?.FullName ?? TypeRef!.FullName;

        class EqualityComparer : IEqualityComparer<Mo>
        {
            public bool Equals(Mo x, Mo y)
            {
                var argsX = ExtractTypeArgs(x, out var typeX);
                var argsY = ExtractTypeArgs(y, out var typeY);
                if (typeX.FullName != typeY.FullName || argsX.Count != argsY.Count) return false;
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
                var args = ExtractTypeArgs(obj, out var type);
                var props = ExtractTypeProperties(obj);
                var hash = type.GetHashCode();
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

            private Collection<CustomAttributeArgument> ExtractTypeArgs(Mo mo, out TypeReference typeRef)
            {
                if (mo.Attribute != null)
                {
                    typeRef = mo.Attribute.AttributeType;
                    return mo.Attribute.ConstructorArguments;
                }
                typeRef = mo.TypeRef!;
                return [];
            }

            private Collection<CustomAttributeNamedArgument> ExtractTypeProperties(Mo mo)
            {
                return mo.Attribute == null ? [] : mo.Attribute.Properties;
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
