﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Rougamo.Analyzers.Upgradation.PropertyRules;
using System.Collections.Immutable;
using System.Linq;

namespace Rougamo.Analyzers.Upgradation
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Version4To5Analyzer : DiagnosticAnalyzer
    {
        private const string INTERFACE_IMo = "Rougamo.IMo";

        internal const string CATEGORY = "Rougamo";
        internal const string RELEASE_5_URI = "https://github.com/inversionhourglass/Rougamo/releases/tag/v5.0.0";

        private readonly IPropertyRule[] _rules = [new PatternRule(), new FlagsRule(), new OrderRule(), new FeaturesRule(), new ForceSyncRule(), new OmitsRule()];

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rules.Select(x => x.GetRules()).SelectMany(x => x).ToArray());

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            var semanticModel = context.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            var ruleMap = _rules.ToDictionary(x => x.PropertyName, x => x);
            if (classSymbol.AllInterfaces.Any(x => x.ToString() == INTERFACE_IMo))
            {
                foreach (var member in classDeclaration.Members)
                {
                    if (member is not PropertyDeclarationSyntax propertyDeclaration) continue;

                    var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);
                    if (!ruleMap.TryGetValue(propertySymbol.Name, out var rule) || !rule.PropertyType.IsEqual(propertySymbol.Type)) continue;

                    if (classSymbol.AllInterfaces.Any(x => x.ToString() == rule.FlexibleInterfaceName)) continue;

                    var dd = rule.FlexibleRule != null && propertyDeclaration.Modifiers.Any(SyntaxKind.NewKeyword) ? rule.FlexibleRule : rule.Rule;
                    var diagnostic = Diagnostic.Create(dd, propertySymbol.Locations[0], classDeclaration.Identifier.Text);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
