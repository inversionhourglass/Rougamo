using Microsoft.CodeAnalysis;
using static Rougamo.Analyzers.Upgradation.Version4To5Analyzer;

namespace Rougamo.Analyzers.Upgradation.PropertyRules
{
    internal class OrderRule : IPropertyRule
    {
        private static readonly LocalizableString _Title = R.S(nameof(Resources.OrderRuleTitle));
        private static readonly LocalizableString _FlexibleMessageFormat = R.S(nameof(Resources.OrderFlexibleRuleMessage));
        private static readonly LocalizableString _Description = R.S(nameof(Resources.OrderRuleDescription));

        public string PropertyName => "Order";

        public SimpleSymbolType PropertyType => SpecialType.System_Double;

        public DiagnosticDescriptor? Rule => null;

        public DiagnosticDescriptor? FlexibleRule => new(IDs.OBSOLETED_IMO_ORDER_FLEXIBLE, _Title, _FlexibleMessageFormat, CATEGORY, DiagnosticSeverity.Error, true, _Description, RELEASE_5_URI);

        public TypeName? FlexibleInterfaceName => "Rougamo.Flexibility.IFlexibleOrderable";

        public AttributeName? AttributeName => null;

        public IAttributeActivator? AttributeActivator => null;
    }
}
