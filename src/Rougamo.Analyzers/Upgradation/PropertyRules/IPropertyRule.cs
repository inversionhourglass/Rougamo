using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Analyzers.Upgradation.PropertyRules
{
    internal interface IPropertyRule
    {
        string PropertyName { get; }

        SimpleSymbolType PropertyType { get; }

        DiagnosticDescriptor Rule { get; }

        DiagnosticDescriptor? FlexibleRule { get; }

        string FlexibleInterfaceName { get; }
    }

    internal static class IPropertyRuleExtensions
    {
        public static DiagnosticDescriptor[] GetRules(this IPropertyRule rule)
        {
            var rules = new HashSet<DiagnosticDescriptor>();
            if (rule.Rule != null) rules.Add(rule.Rule);
            if (rule.FlexibleRule != null) rules.Add(rule.FlexibleRule);

            return rules.ToArray();
        }
    }
}
