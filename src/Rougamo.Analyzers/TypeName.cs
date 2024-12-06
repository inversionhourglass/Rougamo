namespace Rougamo.Analyzers
{
    internal readonly struct TypeName(string fullName)
    {
        public string FullName { get; } = fullName;

        public string Name { get; } = fullName.Substring(fullName.LastIndexOf('.') + 1);

        public string Namespace { get; } = fullName.Substring(0, fullName.LastIndexOf('.'));

        public static implicit operator string(TypeName attributeName) => attributeName.FullName;

        public static implicit operator TypeName(string fullName) => new(fullName);
    }
}
