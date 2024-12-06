using Microsoft.CodeAnalysis;
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

        internal static readonly IPropertyRule[] Rules = [new PatternRule(), new FlagsRule(), new OrderRule(), new FeaturesRule(), new ForceSyncRule(), new OmitsRule()];

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rules.Select(x => x.GetRules()).SelectMany(x => x).ToArray());

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            var semanticModel = context.SemanticModel;
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);

            var ruleMap = Rules.ToDictionary(x => x.PropertyName, x => x);
            if (typeSymbol.AllInterfaces.Any(x => x.ToString() == INTERFACE_IMo))
            {
                foreach (var member in typeDeclaration.Members)
                {
                    if (member is not PropertyDeclarationSyntax propertyDeclaration) continue;

                    var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);
                    if (!ruleMap.TryGetValue(propertySymbol.Name, out var rule) || !rule.PropertyType.IsEqual(propertySymbol.Type)) continue;

                    if (typeSymbol.AllInterfaces.Any(x => x.ToString() == rule.FlexibleInterfaceName)) continue;

                    var dd = rule.Rule;
                    if (dd == null || (rule.FlexibleRule != null && propertyDeclaration.Modifiers.Any(SyntaxKind.NewKeyword)))
                    {
                        dd = rule.FlexibleRule;
                    }
                    var diagnostic = Diagnostic.Create(dd, propertySymbol.Locations[0], typeDeclaration.Identifier.Text);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
