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
            else if ((modifier & Modifier.Protected) != 0)
            {
                sb.Append("protected ");
            }
            else if ((modifier & Modifier.Public) != 0)
            {
                sb.Append("public ");
            }
            else if ((modifier & Modifier.Internal) != 0)
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
