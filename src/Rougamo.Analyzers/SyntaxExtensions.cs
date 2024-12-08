using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rougamo.Analyzers.Reflection;
using System;
using System.Linq;

namespace Rougamo.Analyzers
{
    internal static class SyntaxExtensions
    {
        public static CompilationUnitSyntax AddNamespace(this SyntaxNode root, string @namespace)
        {
            if (root is not CompilationUnitSyntax compilationUnit) throw new ArgumentException("Root must be a CompilationUnitSyntax", nameof(root));

            if (!compilationUnit.Usings.Any(u => u.Name.ToString() == @namespace))
            {
                var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(@namespace));
                compilationUnit = compilationUnit.AddUsings(usingDirective);
            }

            return compilationUnit;
        }

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

        public static bool HasParameterlessConstructor(this TypeDeclarationSyntax typeDeclaration)
        {
            if (typeDeclaration.ParameterList != null) return typeDeclaration.ParameterList.Parameters.Count == 0;

            var constructorDeclarations = typeDeclaration.Members.OfType<ConstructorDeclarationSyntax>();

            return !constructorDeclarations.Any() || constructorDeclarations.Any(x => x.ParameterList.Parameters.Count == 0);
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

        public static AttributeSyntax WithArguments(this AttributeSyntax attribute, params AttributeArgumentSyntax[] arguments)
        {
            var argumentList = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(arguments));

            return attribute.WithArgumentList(argumentList);
        }

        public static PropertyDeclarationSyntax RemoveKeywords(this PropertyDeclarationSyntax propertyDeclaration, params SyntaxKind[] keywords)
        {
            var newModifiers = propertyDeclaration.Modifiers.Where(x => !keywords.Contains(x.Kind()));
            return propertyDeclaration.WithModifiers(SyntaxFactory.TokenList(newModifiers));
        }

        public static PropertyDeclarationSyntax WithDefaultGetterSetter(this PropertyDeclarationSyntax propertyDeclaration)
        {
            return propertyDeclaration.WithAccessorList(SyntaxFactoryPlus.DefaultGetterSetter());
        }

        public static PropertyDeclarationSyntax WithInitializedValue(this PropertyDeclarationSyntax propertyDeclaration, ExpressionSyntax value)
        {
            return propertyDeclaration.WithInitializer(SyntaxFactory.EqualsValueClause(value)).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
    }
}
