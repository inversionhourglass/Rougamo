using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Rougamo.Analyzers
{
    internal static class SyntaxExtensions
    {
        public static PropertyDeclarationSyntax? GetProperty(this TypeDeclarationSyntax typeDeclaration, string propertyName)
        {
            return typeDeclaration.Members.OfType<PropertyDeclarationSyntax>().FirstOrDefault(p => p.Identifier.Text == propertyName);
        }

        public static AttributeSyntax? GetAttribute(this TypeDeclarationSyntax typeDeclaration, string attributeFullName, SemanticModel semanticModel)
        {
            return typeDeclaration.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(x => semanticModel.GetSymbolInfo(x).Symbol is IMethodSymbol symbol && symbol.ContainingType.ToDisplayString() == attributeFullName);
        }

        public static AttributeSyntax WithSingleArgument(this AttributeSyntax attribute, AttributeArgumentSyntax argument)
        {
            var arguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(argument));
            return attribute.WithArgumentList(arguments);
        }

        public static AttributeArgumentSyntax AsAttributeArgument(this ExpressionSyntax expression)
        {
            return SyntaxFactory.AttributeArgument(expression);
        }
    }
}
