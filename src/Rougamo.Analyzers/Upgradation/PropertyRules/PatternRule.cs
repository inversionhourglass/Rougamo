using Microsoft.CodeAnalysis;
using Rougamo.Analyzers.Reflection;
using static Rougamo.Analyzers.Upgradation.Version4To5Analyzer;

namespace Rougamo.Analyzers.Upgradation.PropertyRules
{
    internal class PatternRule : IPropertyRule
    {
        private static readonly LocalizableString _Title = R.S(nameof(Resources.V4t5PatternRuleTitle));
        private static readonly LocalizableString _MessageFormat = R.S(nameof(Resources.V4t5PatternRuleMessage));
        private static readonly LocalizableString _FlexibleMessageFormat = R.S(nameof(Resources.V4t5PatternFlexibleRuleMessage));
        private static readonly LocalizableString _Description = R.S(nameof(Resources.V4t5PatternRuleDescription));

        public string PropertyName => "Pattern";

        public SimpleSymbolType PropertyType => SpecialType.System_String;

        public DiagnosticDescriptor? Rule => new(IDs.OBSOLETED_IMO_PATTERN, _Title, _MessageFormat, CATEGORY, DiagnosticSeverity.Error, true, _Description, RELEASE_5_URI);

        public DiagnosticDescriptor? FlexibleRule => new(IDs.OBSOLETED_IMO_PATTERN_FLEXIBLE, _Title, _FlexibleMessageFormat, CATEGORY, DiagnosticSeverity.Error, true, _Description, RELEASE_5_URI);

        public TypeName? FlexibleInterfaceName => "Rougamo.Flexibility.IFlexiblePatternPointcut";

        public AttributeName? AttributeName => "Rougamo.Metadatas.PointcutAttribute";

        public IAttributeActivator? AttributeActivator => new CtorAttributeActivator();
    }
}
