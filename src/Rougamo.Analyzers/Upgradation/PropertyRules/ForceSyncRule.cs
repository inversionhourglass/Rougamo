using Microsoft.CodeAnalysis;
using Rougamo.Analyzers.Reflection;
using static Rougamo.Analyzers.Upgradation.Version4To5Analyzer;

namespace Rougamo.Analyzers.Upgradation.PropertyRules
{
    internal class ForceSyncRule : IPropertyRule
    {
        private static readonly LocalizableString _Title = R.S(nameof(Resources.V4t5ForceSyncRuleTitle));
        private static readonly LocalizableString _MessageFormat = R.S(nameof(Resources.V4t5ForceSyncRuleMessage));
        private static readonly LocalizableString _Description = R.S(nameof(Resources.V4t5ForceSyncRuleDescription));

        public string PropertyName => "ForceSync";

        public SimpleSymbolType PropertyType => "Rougamo.ForceSync";

        public DiagnosticDescriptor? Rule => new(IDs.OBSOLETED_IMO_FORCESYNC, _Title, _MessageFormat, R.CATEGORY, DiagnosticSeverity.Error, true, _Description, RELEASE_5_URI);

        public DiagnosticDescriptor? FlexibleRule => null;

        public TypeName? FlexibleInterfaceName => null;

        public AttributeName? AttributeName => "Rougamo.Metadatas.OptimizationAttribute";

        public IAttributeActivator? AttributeActivator => new PropAttributeActivator("ForceSync");
    }
}
