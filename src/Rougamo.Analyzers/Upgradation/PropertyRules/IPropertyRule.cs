using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Rougamo.Analyzers.Upgradation.PropertyRules
{
    internal interface IPropertyRule
    {
        string PropertyName { get; }

        SimpleSymbolType PropertyType { get; }

        DiagnosticDescriptor? Rule { get; }

        DiagnosticDescriptor? FlexibleRule { get; }

        TypeName? FlexibleInterfaceName { get; }

        AttributeName? AttributeName { get; }

        IAttributeActivator? AttributeActivator { get; }
    }

    internal static class IPropertyRuleExtensions
    {
        public static DiagnosticDescriptor[] GetRules(this IPropertyRule rule)
        {
            var rules = new List<DiagnosticDescriptor>();

            if (rule.Rule != null) rules.Add(rule.Rule);
            if (rule.FlexibleRule != null) rules.Add(rule.FlexibleRule);

            return rules.ToArray();
        }
    }
}
