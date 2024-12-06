using Microsoft.CodeAnalysis;
using static Rougamo.Analyzers.Upgradation.Version4To5Analyzer;

namespace Rougamo.Analyzers.Upgradation.PropertyRules
{
    internal class PatternRule : IPropertyRule
    {
        private static readonly LocalizableString _Title = R.S(nameof(Resources.PatternRuleTitle));
        private static readonly LocalizableString _MessageFormat = R.S(nameof(Resources.PatternRuleMessage));
        private static readonly LocalizableString _FlexibleMessageFormat = R.S(nameof(Resources.PatternFlexibleRuleMessage));
        private static readonly LocalizableString _Description = R.S(nameof(Resources.PatternRuleDescription));

        public const string NAME = "Pattern";
        public static readonly AttributeName Attribute = "Rougamo.Metadatas.PointcutAttribute";

        public string PropertyName => NAME;

        public SimpleSymbolType PropertyType => SpecialType.System_String;

        public DiagnosticDescriptor Rule => new(IDs.OBSOLETED_IMO_PATTERN, _Title, _MessageFormat, CATEGORY, DiagnosticSeverity.Error, true, _Description, RELEASE_5_URI);

        public DiagnosticDescriptor? FlexibleRule => new(IDs.OBSOLETED_IMO_PATTERN_FLEXIBLE, _Title, _FlexibleMessageFormat, CATEGORY, DiagnosticSeverity.Error, true, _Description, RELEASE_5_URI);

        public string FlexibleInterfaceName => "Rougamo.Flexibility.IFlexiblePatternPointcut";
    }
}
