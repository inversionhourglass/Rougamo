using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rougamo.Analyzers.Upgradation.PropertyRules;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rougamo.Analyzers.Upgradation
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Version4To5CodeFixProvider)), Shared]
    public class Version4To5CodeFixProvider : CodeFixProvider
    {
        private const string FIX_REPLACE_WITH_ATTRIBUTE = "Replace with Attribute";
        private const string FIX_REPLACE_WITH_FLEXIBLE_INTERFACE = "Replace with flexible interface";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Version4To5Analyzer.Rules.Select(x => x.GetRules()).SelectMany(x => x).Select(x => x.Id).ToArray());

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

                switch (diagnostic.Id)
                {
                    case IDs.OBSOLETED_IMO_PATTERN:
                        context.RegisterCodeFix(CodeAction.Create(FIX_REPLACE_WITH_ATTRIBUTE, token => FixPatternWithAttributeAsync(context.Document, declaration, token)), diagnostic);
                        break;
                    case IDs.OBSOLETED_IMO_PATTERN_FLEXIBLE:
                        context.RegisterCodeFix(CodeAction.Create(FIX_REPLACE_WITH_FLEXIBLE_INTERFACE, token => FixPatternWithFlexibleInterfaceAsync(context.Document, declaration, token)), diagnostic);
                        break;
                    case IDs.OBSOLETED_IMO_FLAGS:
                        break;
                    case IDs.OBSOLETED_IMO_FLAGS_FLEXIBLE:
                        break;
                    case IDs.OBSOLETED_IMO_ORDER_FLEXIBLE:
                        break;
                    case IDs.OBSOLETED_IMO_FEATURES:
                        break;
                    case IDs.OBSOLETED_IMO_OMITS:
                        break;
                    case IDs.OBSOLETED_IMO_FORCESYNC:
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected diagnostic id: {diagnostic.Id}");
                }
            }
        }

        private async Task<Document> FixPatternWithAttributeAsync(Document document, TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            var patternDeclaration = typeDeclaration.GetProperty(PatternRule.NAME);

            if (patternDeclaration == null) return document;

            var patternSyntaxToken = patternDeclaration.GetDefaultValue();

            var newTypeDeclaration = typeDeclaration.RemoveNode(patternDeclaration, SyntaxRemoveOptions.KeepNoTrivia);
            var addAttribute = patternSyntaxToken.HasValue && patternSyntaxToken.Value.Value != null;
            if (addAttribute)
            {// Pattern有默认值，需要将配置转移到PointcutAttribute中
                var patternValue = patternSyntaxToken!.Value.ValueText;
                var attributeArgument = SyntaxFactoryPlus.StringLiteralExpression(patternValue).AsAttributeArgument();
                var attribute = typeDeclaration.GetAttribute(PatternRule.Attribute, semanticModel);

                if (attribute != null)
                {// 已存在PointcutAttribute，直接替换
                    var newAttribute = attribute.WithSingleArgument(attributeArgument);
                    var attributeLists = typeDeclaration.AttributeLists.Replace(attribute, newAttribute);
                    newTypeDeclaration = newTypeDeclaration.WithAttributeLists(attributeLists);
                }
                else
                {
                    attribute = SyntaxFactoryPlus.Attribute(PatternRule.Attribute.ShortName, attributeArgument);
                    newTypeDeclaration = newTypeDeclaration.AddAttribute(attribute);
                }
            }

            var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration);

            if (addAttribute) newRoot = newRoot.AddNamespace(PatternRule.Attribute.Namespace);

            return document.WithSyntaxRoot(newRoot);
        }

        private async Task<Document> FixPatternWithFlexibleInterfaceAsync(Document document, TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
        {
            var patternDeclaration = typeDeclaration.GetProperty(PatternRule.NAME);

            if (patternDeclaration == null) return document;

            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var newPatternDeclaration = patternDeclaration.RemoveKeywords(SyntaxKind.NewKeyword);
            var newTypeDeclaration = typeDeclaration.ReplaceNode(patternDeclaration, newPatternDeclaration);

            newTypeDeclaration = newTypeDeclaration.AddInterface(PatternRule.FlexibleInterface);

            var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration).AddNamespace(PatternRule.FlexibleInterface.Namespace);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
