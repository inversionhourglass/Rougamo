using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using Rougamo.Analyzers.Upgradation;
using System.Collections.Immutable;
using System.Reflection;

namespace Rougamo.Analyzers.Tests
{
    public class AssemblyTests
    {
        [Fact]
        public async Task Version4To5Test()
        {
            using var workspace = MSBuildWorkspace.Create();
            var project = await GetProjectAsync(workspace);
            var diagnostics = await GetDiagnosticsAsync<Version4To5Analyzer>(workspace, project);

            Assert.NotEmpty(diagnostics);
            Assert.Contains(diagnostics, x => IsMatch(x, "OverrideOnlyAttribute.cs", IDs.OBSOLETED_IMO_PATTERN));
            Assert.Contains(diagnostics, x => IsMatch(x, "OverrideOnlyAttribute.cs", IDs.OBSOLETED_IMO_FLAGS));
            Assert.Contains(diagnostics, x => IsMatch(x, "OverrideOnlyAttribute.cs", IDs.OBSOLETED_IMO_FEATURES));
            Assert.Contains(diagnostics, x => IsMatch(x, "OverrideOnlyAttribute.cs", IDs.OBSOLETED_IMO_OMITS));
            Assert.Contains(diagnostics, x => IsMatch(x, "OverrideOnlyAttribute.cs", IDs.OBSOLETED_IMO_FORCESYNC));
            Assert.Contains(diagnostics, x => IsMatch(x, "StructMo.cs", IDs.OBSOLETED_IMO_PATTERN));
            Assert.Contains(diagnostics, x => IsMatch(x, "StructMo.cs", IDs.OBSOLETED_IMO_FLAGS));
            Assert.Contains(diagnostics, x => IsMatch(x, "StructMo.cs", IDs.OBSOLETED_IMO_FEATURES));
            Assert.Contains(diagnostics, x => IsMatch(x, "StructMo.cs", IDs.OBSOLETED_IMO_OMITS));
            Assert.Contains(diagnostics, x => IsMatch(x, "StructMo.cs", IDs.OBSOLETED_IMO_FORCESYNC));
            Assert.Contains(diagnostics, x => IsMatch(x, "WithNewKeywordAttribute.cs", IDs.OBSOLETED_IMO_PATTERN_FLEXIBLE));
            Assert.Contains(diagnostics, x => IsMatch(x, "WithNewKeywordAttribute.cs", IDs.OBSOLETED_IMO_FLAGS_FLEXIBLE));
            Assert.Contains(diagnostics, x => IsMatch(x, "WithNewKeywordAttribute.cs", IDs.OBSOLETED_IMO_ORDER_FLEXIBLE));

            //await TestCodeFix(project, diagnostics, "OverrideOnlyAttribute.cs", IDs.OBSOLETED_IMO_PATTERN);
            await TestCodeFix(project, diagnostics, "StructMo.cs", IDs.OBSOLETED_IMO_ORDER_FLEXIBLE);
            //await TestCodeFix(project, diagnostics, "WithNewKeywordAttribute.cs", IDs.OBSOLETED_IMO_PATTERN);
        }

        [Fact]
        public async Task LifetimeTest()
        {
            using var workspace = MSBuildWorkspace.Create();
            var project = await GetProjectAsync(workspace);
            var diagnostics = await GetDiagnosticsAsync<LifetimeAttributeAnalyzer>(workspace, project);

            Assert.NotEmpty(diagnostics);
            Assert.Contains(diagnostics, x => IsMatch(x, "PooledAttribute.cs", IDs.LIFETIME_UNEXPECTED_ARGUMENTS));
            Assert.Contains(diagnostics, x => IsMatch(x, "SingletonAttribute.cs", IDs.LIFETIME_UNEXPECTED_ARGUMENTS));
            Assert.Contains(diagnostics, x => IsMatch(x, "SingletonAttribute.cs", IDs.LIFETIME_UNEXPECTED_PROPERTY));
            Assert.Contains(diagnostics, x => IsMatch(x, "StructMo.cs", IDs.LIFETIME_STRUCT_UNSUPPORTED));
        }

        private static async Task<Project> GetProjectAsync(MSBuildWorkspace workspace)
        {
            return await workspace.OpenProjectAsync("../../../../TestAssemblies/AnalyzerTestAssembly/AnalyzerTestAssembly.csproj");
        }

        private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync<TAnalyzer>(MSBuildWorkspace workspace, Project project) where TAnalyzer : DiagnosticAnalyzer, new()
        {
            AddCompilationConstants(project);
            var compilation = await project.GetCompilationAsync();

            var analyzer = new TAnalyzer();
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);
            var compilationWithAnalyzers = compilation!.WithAnalyzers(analyzers);

            return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        }

        private static async Task TestCodeFix(Project project, ImmutableArray<Diagnostic> diagnostics, string fileName, string diagnosticId)
        {
            var fixProvider = new Version4To5CodeFixProvider();
            var document = project.Documents.First(x => x.Name == fileName);
            var expectedDocument = project.Documents.First(x => x.Name == $"Expected{fileName}");
            var diagnostic = diagnostics.First(x => IsMatch(x, fileName, diagnosticId));

            var actions = new List<CodeAction>();
            var fixContext = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
            await fixProvider.RegisterCodeFixesAsync(fixContext);

            var codeAction = actions.First();
            var operations = await codeAction.GetOperationsAsync(CancellationToken.None);

            var applyChangesOperation = operations.OfType<ApplyChangesOperation>().First();
            var newSolution = applyChangesOperation.ChangedSolution;

            var newDocument = newSolution.GetDocument(document.Id)!;
            var newText = await newDocument.GetTextAsync();
            var expectedText = await expectedDocument.GetTextAsync();

            //Assert.Equal(expectedText.ToString(), newText.ToString());
        }

        private static bool IsMatch(Diagnostic diagnostic, string fileName, string id)
        {
            return diagnostic.Id == id && diagnostic.Location.IsInSource && Path.GetFileName(diagnostic.Location.SourceTree.FilePath) == fileName;
        }

        private static void AddCompilationConstants(Project project)
        {
            var parseOptions = project.ParseOptions!;
            var pPreprocessorSymbols = parseOptions.GetType().GetProperty("PreprocessorSymbols", BindingFlags.NonPublic | BindingFlags.Instance);
            var names = ImmutableArray.Create(parseOptions.PreprocessorSymbolNames.Concat(["ALLOWED_COMPILER_ERROR"]).ToArray());
            pPreprocessorSymbols!.SetValue(parseOptions, names);
        }
    }
}
