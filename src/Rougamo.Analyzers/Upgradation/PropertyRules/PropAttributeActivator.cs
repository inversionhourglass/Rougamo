using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Rougamo.Analyzers.Upgradation.PropertyRules
{
    internal class PropAttributeActivator(string propertyName) : IAttributeActivator
    {
        public AttributeSyntax New(string attributeTypeName, ExpressionSyntax argument)
        {
            var namedArgument = SyntaxFactoryPlus.AttributePropertyArgument(propertyName, argument);
            return SyntaxFactoryPlus.Attribute(attributeTypeName, namedArgument);
        }

        public AttributeSyntax Replace(AttributeSyntax attribute, ExpressionSyntax argument)
        {
            var set = false;
            var newArguments = new List<AttributeArgumentSyntax>();
            var namedArgument = SyntaxFactoryPlus.AttributePropertyArgument(propertyName, argument);
            if (attribute.ArgumentList != null)
            {
                foreach (var arg in attribute.ArgumentList.Arguments)
                {
                    if (arg.NameEquals?.Name.Identifier.Text == propertyName)
                    {
                        newArguments.Add(namedArgument);
                        set = true;
                    }
                    else
                    {
                        newArguments.Add(arg);
                    }
                }
            }
            if (!set) newArguments.Add(namedArgument);

            return attribute.WithArguments(newArguments.ToArray());
        }
    }
}
