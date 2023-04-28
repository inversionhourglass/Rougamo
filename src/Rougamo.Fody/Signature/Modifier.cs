using System;
using System.Text;

namespace Rougamo.Fody.Signature
{
    [Flags]
    public enum Modifier
    {
        Private = 0x1,
        Protected = 0x2,
        Public = 0x4,
        Internal = 0x8,
        Static = 0x10
    }

    public static class ModifierExtensions
    {
        public static string ToDefinitionString(this Modifier modifier)
        {
            var sb = new StringBuilder();
            if ((modifier & Modifier.Private) != 0)
            {
                sb.Append("private ");
            }
            if ((modifier & Modifier.Protected) != 0)
            {
                sb.Append("protected ");
            }
            if ((modifier & Modifier.Public) != 0)
            {
                sb.Append("public ");
            }
            if ((modifier & Modifier.Internal) != 0)
            {
                sb.Append("internal ");
            }
            if ((modifier & Modifier.Static) != 0)
            {
                sb.Append("static ");
            }
            return sb.ToString(0, sb.Length - 1);
        }
    }
}
