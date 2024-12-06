using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rougamo.Analyzers.Upgradation.PropertyRules;
using System;
using System.Collections.Generic;
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

            var ruleMap = Version4To5Analyzer.Rules.Select(x =>
            {
                var list = new List<FixRule>();
                if (x.Rule != null) list.Add(new(x.Rule.Id, FixRuleType.Attribute, x));
                if (x.FlexibleRule != null) list.Add(new(x.FlexibleRule.Id, FixRuleType.Interface, x));
                return list;
            }).SelectMany(x => x).ToDictionary(x => x.Id, x => x);
            foreach (var diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

                if (!ruleMap.TryGetValue(diagnostic.Id, out var rule)) throw new InvalidOperationException($"Unexpected diagnostic id: {diagnostic.Id}");

                if (rule.Type == FixRuleType.Attribute)
                {
                    context.RegisterCodeFix(CodeAction.Create(FIX_REPLACE_WITH_ATTRIBUTE, token => FixWithAttributeAsync(context.Document, declaration, rule.PropertyRule, token)), diagnostic);
                }
                else
                {
                    context.RegisterCodeFix(CodeAction.Create(FIX_REPLACE_WITH_FLEXIBLE_INTERFACE, token => FixWithFlexibleInterfaceAsync(context.Document, declaration, rule.PropertyRule, token)), diagnostic);
                }
            }
        }

        private async Task<Document> FixWithAttributeAsync(Document document, TypeDeclarationSyntax typeDeclaration, IPropertyRule rule, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            var propertyDeclaration = typeDeclaration.GetProperty(rule.PropertyName);

            if (propertyDeclaration == null) return document;

            var attributeName = rule.AttributeName!.Value;
            var propertyValue = propertyDeclaration.Initializer?.Value ?? propertyDeclaration.ExpressionBody?.Expression;

            var newTypeDeclaration = typeDeclaration.RemoveNode(propertyDeclaration, SyntaxRemoveOptions.KeepNoTrivia);
            if (propertyValue != null)
            {// 属性有默认值，需要将配置转移到对应的Attribute中
                var attribute = typeDeclaration.GetAttribute(attributeName, semanticModel);

                if (attribute != null)
                {// 已存在Attribute，直接替换
                    var newAttribute = rule.AttributeActivator!.Replace(attribute, propertyValue);
                    var attributeLists = typeDeclaration.AttributeLists.Replace(attribute, newAttribute);
                    newTypeDeclaration = newTypeDeclaration.WithAttributeLists(attributeLists);
                }
                else
                {
                    attribute = rule.AttributeActivator!.New(attributeName.ShortName, propertyValue);
                    newTypeDeclaration = newTypeDeclaration.AddAttribute(attribute);
                }
            }

            var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration);

            if (propertyValue != null) newRoot = newRoot.AddNamespace(attributeName.Namespace);

            return document.WithSyntaxRoot(newRoot);
        }

        private async Task<Document> FixWithFlexibleInterfaceAsync(Document document, TypeDeclarationSyntax typeDeclaration, IPropertyRule rule, CancellationToken cancellationToken)
        {
            var propertyDeclaration = typeDeclaration.GetProperty(rule.PropertyName);

            if (propertyDeclaration == null) return document;

            var interfaceName = rule.FlexibleInterfaceName!.Value;
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var newPropertyDeclaration = propertyDeclaration.RemoveKeywords(SyntaxKind.NewKeyword, SyntaxKind.OverrideKeyword);
            var newTypeDeclaration = typeDeclaration.ReplaceNode(propertyDeclaration, newPropertyDeclaration);

            newTypeDeclaration = newTypeDeclaration.AddInterface(interfaceName);

            var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration).AddNamespace(interfaceName.Namespace);

            return document.WithSyntaxRoot(newRoot);
        }

        private record struct FixRule(string Id, FixRuleType Type, IPropertyRule PropertyRule);

        private enum FixRuleType
        {
            Attribute,
            Interface
        }
    }
}
