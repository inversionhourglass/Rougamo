using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Signature
{
    /// <summary>
    /// Type signature
    /// </summary>
    public class TypeSignature
    {
        public const char NESTED_SEPARATOR = '/';

        public static readonly TypeSignature[] EmptyArray = new TypeSignature[0];

        /// <summary/>
        public TypeSignature(string @namespace, IReadOnlyCollection<GenericSignature> nestedTypes, TypeReference reference)
        {
            Namespace = @namespace;
            NestedTypes = nestedTypes.ToArray();
            Reference = reference;
        }

        public string Namespace { get; }

        public GenericSignature[] NestedTypes { get; }

        public TypeReference Reference { get; }

        public override string ToString()
        {
            return $"{Namespace}.{string.Join(NESTED_SEPARATOR.ToString(), NestedTypes.AsEnumerable())}";
        }
    }
}
