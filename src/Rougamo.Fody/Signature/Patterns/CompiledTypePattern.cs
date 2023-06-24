using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Signature.Patterns
{
    public class CompiledTypePattern : ITypePattern
    {
        public CompiledTypePattern(INamespacePattern @namespace, IGenericNamePatterns nestedTypePatterns, bool assignableMatch)
        {
            Namespace = @namespace;
            NestedTypePatterns = nestedTypePatterns;
            AssignableMatch = assignableMatch;
        }

        public INamespacePattern Namespace { get; }

        public IGenericNamePatterns NestedTypePatterns { get; }

        public bool AssignableMatch { get; }

        public virtual bool IsMatch(TypeSignature signature)
        {
            var matched = false;
            if (NestedTypePatterns.Count == signature.NestedTypes.Length)
            {
                if (Namespace.IsMatch(signature.Namespace))
                {
                    matched = NestedTypePatterns.IsMatch(signature.NestedTypes);
                }
            }
            if (matched || !AssignableMatch) return matched;
            return IsAssignableMatch(signature);
        }

        private bool IsAssignableMatch(TypeSignature signature)
        {
            var genericMap = new Dictionary<string, TypeSignature>();
            var typeRef = signature.Reference;
            var typeDef = typeRef.Resolve();

            if (typeDef == null) return false;
            if (typeDef.HasGenericParameters)
            {
                var genericSignature = signature.NestedTypes.SelectMany(x => x.Generics).ToArray();
                for (var i = 0; i < typeDef.GenericParameters.Count; i++)
                {
                    genericMap[typeDef.GenericParameters[i].Name] = genericSignature[i];
                }
            }

            while (true)
            {
                foreach (var @interface in typeDef.Interfaces)
                {
                    if (IsTypeRefMatch(@interface.InterfaceType, genericMap)) return true;
                }

                typeRef = typeDef.BaseType;
                if (typeRef == null) return false;

                if (IsTypeRefMatch(typeRef, genericMap)) return true;
                typeDef = typeRef.Resolve();
            }
        }

        private bool IsTypeRefMatch(TypeReference typeRef, Dictionary<string, TypeSignature> genericMap)
        {
            var signature = SignatureParser.ParseType(typeRef, null, genericMap);
            return IsMatch(signature);
        }

        public static CompiledTypePattern NewAny() => new(new AnyNamespacePattern(), new AnyGenericNamePatterns(), false);

        public static CompiledTypePattern NewPrimitiveOrAnyNs(string shortName, bool assignableMatch)
        {
            var name = shortName switch
            {
                "bool" => typeof(bool).Name,
                "byte" => typeof(byte).Name,
                "sbyte" => typeof(sbyte).Name,
                "char" => typeof(char).Name,
                "decimal" => typeof(decimal).Name,
                "double" => typeof(double).Name,
                "float" => typeof(float).Name,
                "int" => typeof(int).Name,
                "uint" => typeof(uint).Name,
                "long" => typeof(long).Name,
                "ulong" => typeof(ulong).Name,
                "object" => typeof(object).Name,
                "short" => typeof(short).Name,
                "ushort" => typeof(ushort).Name,
                "string" => typeof(string).Name,
                "void" => typeof(void).Name,
                _ => null
            };

            return name == null ? new(new AnyNamespacePattern(), new SingleNonGenericNamePattern(shortName), assignableMatch) : new(new SystemNamespacePattern(), new SingleNonGenericNamePattern(name), assignableMatch);
        }
    }
}
