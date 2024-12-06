using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Rougamo.Analyzers
{
    internal static class SyntaxFactoryPlus
    {
        public static AttributeSyntax Attribute(string name, AttributeArgumentSyntax argument)
        {
            return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(name)).WithArguments(argument);
        }

        public static AttributeArgumentSyntax AttributePropertyArgument(string propertyName, ExpressionSyntax value)
        {
            return SyntaxFactory.AttributeArgument(value).WithNameEquals(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(propertyName)));
        }
    }
}
