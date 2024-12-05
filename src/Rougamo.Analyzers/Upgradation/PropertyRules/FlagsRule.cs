using Microsoft.CodeAnalysis;
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

        public DiagnosticDescriptor Rule => new(IDs.OBSOLETED_IMO_FLAGS, _Title, _MessageFormat, CATEGORY, DiagnosticSeverity.Error, true, _Description, RELEASE_5_URI);

        public DiagnosticDescriptor? FlexibleRule => new(IDs.OBSOLETED_IMO_FLAGS_FLEXIBLE, _Title, _FlexibleMessageFormat, CATEGORY, DiagnosticSeverity.Error, true, _Description, RELEASE_5_URI);

        public string FlexibleInterfaceName => "Rougamo.Flexibility.IFlexibleModifierPointcut";
    }
}
