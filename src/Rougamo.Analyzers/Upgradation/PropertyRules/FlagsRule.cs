using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rougamo.Analyzers.Reflection;
using static Rougamo.Analyzers.Upgradation.Version4To5Analyzer;

namespace Rougamo.Analyzers.Upgradation.PropertyRules
{
    internal class FlagsRule : IPropertyRule
    {
        private static readonly LocalizableString _Title = R.S(nameof(Resources.FlagsRuleTitle));
        private static readonly LocalizableString _MessageFormat = R.S(nameof(Resources.FlagsRuleMessage));
        private static readonly LocalizableString _FlexibleMessageFormat = R.S(nameof(Resources.FlagsFlexibleRuleMessage));
        private static readonly LocalizableString _Description = R.S(nameof(Resources.FlagsRuleDescription));

        public string PropertyName => "Flags";

        public SimpleSymbolType PropertyType => "Rougamo.AccessFlags";

        public DiagnosticDescriptor? Rule => new(IDs.OBSOLETED_IMO_FLAGS, _Title, _MessageFormat, CATEGORY, DiagnosticSeverity.Error, true, _Description, RELEASE_5_URI);

        public DiagnosticDescriptor? FlexibleRule => new(IDs.OBSOLETED_IMO_FLAGS_FLEXIBLE, _Title, _FlexibleMessageFormat, CATEGORY, DiagnosticSeverity.Error, true, _Description, RELEASE_5_URI);

        public TypeName? FlexibleInterfaceName => "Rougamo.Flexibility.IFlexibleModifierPointcut";

        public AttributeName? AttributeName => "Rougamo.Metadatas.PointcutAttribute";

        public IAttributeActivator? AttributeActivator => new FlagsAttributeActivator();

        private class FlagsAttributeActivator : CtorAttributeActivator
        {
            public override AttributeSyntax Replace(AttributeSyntax attribute, ExpressionSyntax argument)
            {
                var arguments = attribute.ArgumentList.Arguments;
                if (arguments.Count == 1 && arguments[0].Expression is LiteralExpressionSyntax literal && literal.Token.Value is string)
                {// 当前已存在的Attribute使用的是pattern，AccessFlags的优先级没有pattern高，不替换，直接返回
                    return attribute;
                }

                return base.Replace(attribute, argument);
            }
        }
    }
}
