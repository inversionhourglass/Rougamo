using Microsoft.CodeAnalysis;
using static Rougamo.Analyzers.Upgradation.Version4To5Analyzer;

namespace Rougamo.Analyzers.Upgradation.PropertyRules
{
    internal class ForceSyncRule : IPropertyRule
    {
        private static readonly LocalizableString _Title = R.S(nameof(Resources.ForceSyncRuleTitle));
        private static readonly LocalizableString _MessageFormat = R.S(nameof(Resources.ForceSyncRuleMessage));
        private static readonly LocalizableString _Description = R.S(nameof(Resources.ForceSyncRuleDescription));

        public string PropertyName => "ForceSync";

        public SimpleSymbolType PropertyType => "Rougamo.ForceSync";

        public DiagnosticDescriptor Rule => new(IDs.OBSOLETED_IMO_FORCESYNC, _Title, _MessageFormat, CATEGORY, DiagnosticSeverity.Error, true, _Description, RELEASE_5_URI);

        public DiagnosticDescriptor? FlexibleRule => null;

        public string FlexibleInterfaceName => string.Empty;
    }
}
