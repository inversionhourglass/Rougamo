using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Rougamo.Analyzers
{
    internal static class SyntaxExtensions
    {
        public static TypeDeclarationSyntax AddAttribute(this TypeDeclarationSyntax typeDeclaration, AttributeSyntax attribute)
        {
            var leadingTrivia = typeDeclaration.GetLeadingTrivia();
            var attributeLists = typeDeclaration.AttributeLists;
            var attributeList = attribute.ToList();
            if (attributeLists.Count == 0 && !string.IsNullOrWhiteSpace(leadingTrivia.ToString()))
            {
                attributeList = attributeList.WithLeadingTrivia(leadingTrivia);
                typeDeclaration = typeDeclaration.WithLeadingTrivia(SyntaxTriviaList.Empty);
            }
            attributeLists = attributeLists.Add(attributeList);

            return typeDeclaration.WithAttributeLists(attributeLists);
        }

        public static TypeDeclarationSyntax AddInterface(this TypeDeclarationSyntax typeDeclaration, TypeName interfaceName)
        {
            var interfaceType = SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(interfaceName.Name));
            var baseList = typeDeclaration.BaseList ?? SyntaxFactory.BaseList();
            var newBaseList = baseList.AddTypes(interfaceType);

            return  typeDeclaration.WithBaseList(newBaseList);
        }

        public static PropertyDeclarationSyntax? GetProperty(this TypeDeclarationSyntax typeDeclaration, string propertyName)
        {
            return typeDeclaration.Members.OfType<PropertyDeclarationSyntax>().FirstOrDefault(p => p.Identifier.Text == propertyName);
        }

        public static AttributeSyntax? GetAttribute(this TypeDeclarationSyntax typeDeclaration, string attributeFullName, SemanticModel semanticModel)
        {
            return typeDeclaration.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(x => semanticModel.GetSymbolInfo(x).Symbol is IMethodSymbol symbol && symbol.ContainingType.ToDisplayString() == attributeFullName);
        }

        public static SyntaxList<AttributeListSyntax> Replace(this SyntaxList<AttributeListSyntax> attributeLists, AttributeSyntax oldAttribute, AttributeSyntax newAttribute)
        {
            var attributeList = (AttributeListSyntax)oldAttribute.Parent;
            var newAttributeList = attributeList.ReplaceNode(oldAttribute, newAttribute);
            return attributeLists.Replace(attributeList, newAttributeList);
        }

        public static AttributeListSyntax ToList(this AttributeSyntax attribute)
        {
            return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute));
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

        public static SyntaxToken? GetDefaultValue(this PropertyDeclarationSyntax propertyDeclaration)
        {
            // public string Pattern { get; set; } = "";
            if (propertyDeclaration.Initializer?.Value is LiteralExpressionSyntax les) return les.Token;

            // public string Pattern => "";
            if (propertyDeclaration.ExpressionBody?.Expression is LiteralExpressionSyntax les2) return les2.Token;

            return null;
        }

        public static PropertyDeclarationSyntax RemoveKeywords(this PropertyDeclarationSyntax propertyDeclaration, params SyntaxKind[] keywords)
        {
            var newModifiers = propertyDeclaration.Modifiers.Where(x => !keywords.Contains(x.Kind()));
            return propertyDeclaration.WithModifiers(SyntaxFactory.TokenList(newModifiers));
        }
    }
}
