using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Rougamo.Analyzers.Upgradation.PropertyRules
{
    internal interface IAttributeActivator
    {
        AttributeSyntax New(string attributeTypeName, ExpressionSyntax argument);

        AttributeSyntax Replace(AttributeSyntax attribute, ExpressionSyntax argument);
    }
}
