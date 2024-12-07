using Microsoft.CodeAnalysis;
using Rougamo.Analyzers.Reflection;
using static Rougamo.Analyzers.Upgradation.Version4To5Analyzer;

namespace Rougamo.Analyzers.Upgradation.PropertyRules
{
    internal class FeaturesRule : IPropertyRule
    {
        private static readonly LocalizableString _Title = R.S(nameof(Resources.FeaturesRuleTitle));
        private static readonly LocalizableString _MessageFormat = R.S(nameof(Resources.FeaturesRuleMessage));
        private static readonly LocalizableString _Description = R.S(nameof(Resources.FeaturesRuleDescription));

        public string PropertyName => "Features";

        public SimpleSymbolType PropertyType => "Rougamo.Feature";

        public DiagnosticDescriptor? Rule => new(IDs.OBSOLETED_IMO_FEATURES, _Title, _MessageFormat, CATEGORY, DiagnosticSeverity.Error, true, _Description, RELEASE_5_URI);

        public DiagnosticDescriptor? FlexibleRule => null;

        public TypeName? FlexibleInterfaceName => null;

        public AttributeName? AttributeName => "Rougamo.Metadatas.AdviceAttribute";

        public IAttributeActivator? AttributeActivator => new CtorAttributeActivator();
    }
}
