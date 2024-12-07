using Microsoft.CodeAnalysis;
using Rougamo.Analyzers.Reflection;
using static Rougamo.Analyzers.Upgradation.Version4To5Analyzer;

namespace Rougamo.Analyzers.Upgradation.PropertyRules
{
    internal class OmitsRule : IPropertyRule
    {
        private static readonly LocalizableString _Title = R.S(nameof(Resources.OmitsRuleTitle));
        private static readonly LocalizableString _MessageFormat = R.S(nameof(Resources.OmitsRuleMessage));
        private static readonly LocalizableString _Description = R.S(nameof(Resources.OmitsRuleDescription));

        public string PropertyName => "MethodContextOmits";

        public SimpleSymbolType PropertyType => "Rougamo.Context.Omit";

        public DiagnosticDescriptor? Rule => new(IDs.OBSOLETED_IMO_OMITS, _Title, _MessageFormat, CATEGORY, DiagnosticSeverity.Error, true, _Description, RELEASE_5_URI);

        public DiagnosticDescriptor? FlexibleRule => null;

        public TypeName? FlexibleInterfaceName => null;

        public AttributeName? AttributeName => "Rougamo.Metadatas.OptimizationAttribute";

        public IAttributeActivator? AttributeActivator => new PropAttributeActivator("MethodContext");
    }
}
