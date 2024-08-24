namespace Cecil.AspectN
{
    internal static class PrimitiveExtensions
    {
        public static bool IsWhiteSpace(this char ch) => ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r' || ch == '　';
    }
}
