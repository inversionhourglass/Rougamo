using Microsoft.CodeAnalysis;
using Rougamo.Analyzers.Reflection;
using static Rougamo.Analyzers.Upgradation.Version4To5Analyzer;

namespace Rougamo.Analyzers.Upgradation.PropertyRules
{
    internal class OrderRule : IPropertyRule
    {
        private static readonly LocalizableString _Title = R.S(nameof(Resources.V4t5OrderRuleTitle));
        private static readonly LocalizableString _FlexibleMessageFormat = R.S(nameof(Resources.V4t5OrderFlexibleRuleMessage));
        private static readonly LocalizableString _Description = R.S(nameof(Resources.V4t5OrderRuleDescription));

        public string PropertyName => "Order";

        public SimpleSymbolType PropertyType => SpecialType.System_Double;

        public DiagnosticDescriptor? Rule => null;

        public DiagnosticDescriptor? FlexibleRule => new(IDs.OBSOLETED_IMO_ORDER_FLEXIBLE, _Title, _FlexibleMessageFormat, R.CATEGORY, DiagnosticSeverity.Error, true, _Description, RELEASE_5_URI);

        public TypeName? FlexibleInterfaceName => "Rougamo.Flexibility.IFlexibleOrderable";

        public AttributeName? AttributeName => null;

        public IAttributeActivator? AttributeActivator => null;
    }
}
