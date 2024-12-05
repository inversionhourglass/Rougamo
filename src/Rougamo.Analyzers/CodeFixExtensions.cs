using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Rougamo.Analyzers
{
    internal static class CodeFixExtensions
    {
        public static CompilationUnitSyntax AddNamespace(this SyntaxNode root, string @namespace)
        {
            if (root is not CompilationUnitSyntax compilationUnit) throw new ArgumentException("Root must be a CompilationUnitSyntax", nameof(root));

            if (!compilationUnit.Usings.Any(u => u.Name.ToString() == @namespace))
            {
                var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(@namespace));
                compilationUnit = compilationUnit.AddUsings(usingDirective);
            }

            return compilationUnit;
        }
    }
}
