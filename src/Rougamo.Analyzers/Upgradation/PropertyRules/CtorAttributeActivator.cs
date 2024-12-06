using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Rougamo.Analyzers.Upgradation.PropertyRules
{
    internal class CtorAttributeActivator : IAttributeActivator
    {
        public virtual AttributeSyntax New(string attributeTypeName, ExpressionSyntax argument)
        {
            var attributeArgument = SyntaxFactory.AttributeArgument(argument);
            return SyntaxFactoryPlus.Attribute(attributeTypeName, attributeArgument);
        }

        public virtual AttributeSyntax Replace(AttributeSyntax attribute, ExpressionSyntax argument)
        {
            var attributeArgument = SyntaxFactory.AttributeArgument(argument);
            return attribute.WithArguments(attributeArgument);
        }
    }
}
