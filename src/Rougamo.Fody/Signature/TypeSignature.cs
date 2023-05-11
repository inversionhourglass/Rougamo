using Mono.Cecil;

namespace Rougamo.Fody.Signature
{
    /// <summary>
    /// Type signature
    /// </summary>
    public class TypeSignature
    {
        public const char NESTED_SEPARATOR = '/';

        /// <summary/>
        public TypeSignature(string name, TypeSignature? declaringType, TypeCategory category, TypeReference? reference)
        {
            DeclaringType = declaringType;
            Name = name;
            Category = category;
            Reference = reference;
        }

        public TypeSignature? DeclaringType { get; }

        public TypeReference? Reference { get; set; }

        /// <summary>
        /// Type full name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Category
        /// </summary>
        public TypeCategory Category { get; }

        protected virtual char Separator => NESTED_SEPARATOR;

        protected virtual string FormatName => Name;

        public override string ToString()
        {
            return DeclaringType == null ? FormatName : $"{DeclaringType}{DeclaringType.Separator}{FormatName}";
        }
    }
}
