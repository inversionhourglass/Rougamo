namespace Rougamo.Fody.Signature
{
    /// <summary>
    /// Type signature
    /// </summary>
    public class TypeSignature
    {
        /// <summary/>
        public TypeSignature(string name) : this(name, TypeCategory.Simple) { }

        /// <summary/>
        public TypeSignature(string name, TypeCategory category)
        {
            Name = name;
            Category = category;
        }

        /// <summary>
        /// Type full name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Category
        /// </summary>
        public TypeCategory Category { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
