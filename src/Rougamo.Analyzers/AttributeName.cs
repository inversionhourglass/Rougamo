namespace Rougamo.Analyzers
{
    internal readonly struct AttributeName(string fullName)
    {
        public string FullName { get; } = fullName;

        public string ShortName { get; } = TrimAttributeSuffix(fullName.Substring(fullName.LastIndexOf('.') + 1));

        public string Namespace { get; } = fullName.Substring(0, fullName.LastIndexOf('.'));

        private static string TrimAttributeSuffix(string attributeName)
        {
            return attributeName.EndsWith("Attribute") ? attributeName.Substring(0, attributeName.Length - 9) : attributeName;
        }

        public static implicit operator string(AttributeName attributeName) => attributeName.FullName;

        public static implicit operator AttributeName(string fullName) => new(fullName);
    }
}
