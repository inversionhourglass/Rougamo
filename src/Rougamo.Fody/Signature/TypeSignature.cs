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

        public static readonly TypeSignature[] EmptyArray = [];

        /// <summary/>
        public TypeSignature(string @namespace, IReadOnlyCollection<GenericSignature> nestedTypes, IReadOnlyCollection<int> arrayRanks, TypeReference reference)
        {
            Namespace = @namespace;
            NestedTypes = nestedTypes.ToArray();
            ArrayRanks = arrayRanks.ToArray();
            Reference = reference;
        }

        public string Namespace { get; }

        public GenericSignature[] NestedTypes { get; }

        public TypeReference Reference { get; }

        public int[] ArrayRanks { get; }

        public override string ToString()
        {
            var arraySuffix = ArrayRanks.Length == 0 ? string.Empty : string.Concat(ArrayRanks.Select(x => $"[{new string(',', x - 1)}]"));
            return $"{Namespace}.{string.Join(NESTED_SEPARATOR.ToString(), NestedTypes.AsEnumerable())}{arraySuffix}";
        }
    }
}
