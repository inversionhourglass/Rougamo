namespace Rougamo.Fody.Signature
{
    /// <summary>
    /// Type signature
    /// </summary>
    public class TypeSignature
    {
        public const char NESTED_SEPARATOR = '/';

        /// <summary/>
        public TypeSignature(string name, TypeSignature? declaringType, TypeCategory category)
        {
            DeclaringType = declaringType;
            Name = name;
            Category = category;
        }

        public TypeSignature? DeclaringType { get; }

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
