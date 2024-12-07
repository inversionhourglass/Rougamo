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

        public static AccessorListSyntax DefaultGetterSetter() => SyntaxFactory.AccessorList(SyntaxFactory.List([DefaultGetter(), DefaultSetter()]));

        public static AccessorDeclarationSyntax DefaultGetter() => DefaultAccessor(SyntaxKind.GetAccessorDeclaration);

        public static AccessorDeclarationSyntax DefaultSetter() => DefaultAccessor(SyntaxKind.SetAccessorDeclaration);

        private static AccessorDeclarationSyntax DefaultAccessor(SyntaxKind kind) => SyntaxFactory.AccessorDeclaration(kind).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }
}
