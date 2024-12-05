using Microsoft.CodeAnalysis;
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
        public async Task Test()
        {
            using var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync("../../../../TestAssemblies/AnalyzerTestAssembly/AnalyzerTestAssembly.csproj");
            AddCompilationConstants(project);
            var compilation = await project.GetCompilationAsync();
            
            var analyzer = new Version4To5Analyzer();
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);
            var compilationWithAnalyzers = compilation!.WithAnalyzers(analyzers);

            var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

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
        }

        private bool IsMatch(Diagnostic diagnostic, string fileName, string id)
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