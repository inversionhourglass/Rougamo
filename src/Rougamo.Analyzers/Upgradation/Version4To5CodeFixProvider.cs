using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
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
                        context.RegisterCodeFix(CodeAction.Create("Fix obsoleted property", c => ReplacePatternAsync(context.Document, declaration, c)), diagnostic);
                        break;
                    case IDs.OBSOLETED_IMO_PATTERN_FLEXIBLE:
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

        public async Task<Document> ReplacePatternAsync(Document document, TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            var patternDeclaration = typeDeclaration.GetProperty("Pattern");

            if (patternDeclaration == null) return document;

            SyntaxToken? patternSyntaxToken = null;
            if (patternDeclaration.Initializer?.Value is LiteralExpressionSyntax les)
            {// public string Pattern { get; set; } = "";
                patternSyntaxToken = les.Token;
            }
            else if (patternDeclaration.ExpressionBody?.Expression is LiteralExpressionSyntax les2)
            {// public string Pattern => "";
                patternSyntaxToken = les2.Token;
            }

            var newTypeDeclaration = typeDeclaration.RemoveNode(patternDeclaration, SyntaxRemoveOptions.KeepNoTrivia);
            var hasValue = patternSyntaxToken.HasValue && patternSyntaxToken.Value.Value != null;
            if (hasValue)
            {
                var patternValue = patternSyntaxToken!.Value.ValueText;
                var attributeArgument = SyntaxFactoryPlus.StringLiteralExpression(patternValue).AsAttributeArgument();
                var attribute = typeDeclaration.GetAttribute("Rougamo.Metadatas.PointcutAttribute", semanticModel);
                var attributeLists = typeDeclaration.AttributeLists;
                SyntaxList<AttributeListSyntax> newAttributeLists;
                if (attribute != null)
                {
                    var attributeList = (AttributeListSyntax)attribute.Parent;
                    var newAttribute = attribute.WithSingleArgument(attributeArgument);
                    var newAttributeList = attributeList.ReplaceNode(attribute, newAttribute);
                    newAttributeLists = attributeLists.Replace(attributeList, newAttributeList);
                }
                else
                {
                    attribute = SyntaxFactoryPlus.Attribute("Pointcut", attributeArgument);
                    var attributeList = SyntaxFactoryPlus.AlignedAttributeList(typeDeclaration, attribute);
                    newAttributeLists = typeDeclaration.AttributeLists.Insert(0, attributeList);
                }
                newTypeDeclaration = newTypeDeclaration.WithAttributeLists(newAttributeLists);
            }

            var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration);

            if (hasValue) newRoot = newRoot.AddNamespace("Rougamo.Metadatas");

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
