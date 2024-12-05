using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Rougamo.Analyzers
{
    internal static class SyntaxFactoryPlus
    {
        public static LiteralExpressionSyntax StringLiteralExpression(string value) => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));

        public static AttributeSyntax Attribute(string name, AttributeArgumentSyntax argument)
        {
            return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(name)).WithSingleArgument(argument);
        }

        public static AttributeListSyntax AlignedAttributeList(TypeDeclarationSyntax typeDeclaration, AttributeSyntax attribute)
        {
            var leadingTrivia = typeDeclaration.GetLeadingTrivia();
            var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute));
            return attributeList.WithLeadingTrivia(leadingTrivia).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
        }
    }
}
