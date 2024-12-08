using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Rougamo.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LifetimeAttributeAnalyzer : DiagnosticAnalyzer
    {
        private const string URI = "https://github.com/inversionhourglass/Rougamo/wiki/%E6%80%A7%E8%83%BD%E4%BC%98%E5%8C%96#%E5%88%87%E9%9D%A2%E7%B1%BB%E5%9E%8B%E7%94%9F%E5%91%BD%E5%91%A8%E6%9C%9F";
        private const string ATTRIBUTE_FULLNAME = "Rougamo.Metadatas.LifetimeAttribute";

        private static readonly LocalizableString _UnexpectedArgumentsTitle = R.S(nameof(Resources.LifetimeUnexpectedArgumentsTitle));
        private static readonly LocalizableString _UnexpectedArgumentsMessageFormat = R.S(nameof(Resources.LifetimeUnexpectedArgumentsMessage));
        private static readonly LocalizableString _UnexpectedArgumentsDescription = R.S(nameof(Resources.LifetimeUnexpectedArgumentsDescription));

        private static readonly LocalizableString _UnexpectedPropertyTitle = R.S(nameof(Resources.LifetimeUnexpectedPropertyTitle));
        private static readonly LocalizableString _UnexpectedPropertyMessageFormat = R.S(nameof(Resources.LifetimeUnexpectedPropertyMessage));
        private static readonly LocalizableString _UnexpectedPropertyDescription = R.S(nameof(Resources.LifetimeUnexpectedPropertyDescription));

        private static readonly LocalizableString _StructUnsupportedTitle = R.S(nameof(Resources.LifetimeStructUnsupportedTitle));
        private static readonly LocalizableString _StructUnsupportedMessageFormat = R.S(nameof(Resources.LifetimeStructUnsupportedMessage));
        private static readonly LocalizableString _StructUnsupportedDescription = R.S(nameof(Resources.LifetimeStructUnsupportedDescription));

        private static readonly DiagnosticDescriptor _UnexpectedArgumentsRule = new(IDs.LIFETIME_UNEXPECTED_ARGUMENTS, _UnexpectedArgumentsTitle, _UnexpectedArgumentsMessageFormat, R.CATEGORY, DiagnosticSeverity.Error, true, _UnexpectedArgumentsDescription, URI);
        private static readonly DiagnosticDescriptor _UnexpectedSettablePropertyRule = new(IDs.LIFETIME_UNEXPECTED_PROPERTY, _UnexpectedPropertyTitle, _UnexpectedPropertyMessageFormat, R.CATEGORY, DiagnosticSeverity.Warning, true, _UnexpectedPropertyDescription, URI);
        private static readonly DiagnosticDescriptor _StructUnsupportedRule = new(IDs.LIFETIME_STRUCT_UNSUPPORTED, _StructUnsupportedTitle, _StructUnsupportedMessageFormat, R.CATEGORY, DiagnosticSeverity.Error, true, _StructUnsupportedDescription, URI);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_UnexpectedArgumentsRule, _UnexpectedSettablePropertyRule, _StructUnsupportedRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode1, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration);
        }

        private void AnalyzeNode1(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            var semanticModel = context.SemanticModel;
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);

            var lifetimeAttribute = typeSymbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass.ToString() == ATTRIBUTE_FULLNAME);

            if (lifetimeAttribute == null) return;

            var lifetime = (Lifetime)(int)lifetimeAttribute.ConstructorArguments[0].Value;

            if (typeDeclaration is StructDeclarationSyntax)
            {
                var location = lifetimeAttribute.ApplicationSyntaxReference.GetSyntax().GetLocation();
                var diagnostic = Diagnostic.Create(_StructUnsupportedRule, location, typeDeclaration.Identifier.Text);
                context.ReportDiagnostic(diagnostic);
                return;
            }

            if (lifetime == Lifetime.Transient) return;

            var hasParameterlessCtor = typeDeclaration.HasParameterlessConstructor();
            
            if (!hasParameterlessCtor)
            {
                var location = lifetimeAttribute.ApplicationSyntaxReference.GetSyntax().GetLocation();
                var diagnostic = Diagnostic.Create(_UnexpectedArgumentsRule, location, typeDeclaration.Identifier.Text, lifetime.ToString());
                context.ReportDiagnostic(diagnostic);
            }

            if (lifetime != Lifetime.Singleton) return;

            var settableProperties = typeDeclaration.Members.OfType<PropertyDeclarationSyntax>().Where(x => x.AccessorList?.Accessors.Any(y => y.Keyword.Text == "set") == true);

            foreach (var settableProperty in settableProperties)
            {
                var location = settableProperty.GetLocation();
                var diagnostic = Diagnostic.Create(_UnexpectedSettablePropertyRule, location, typeDeclaration.Identifier.Text, lifetime.ToString());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
