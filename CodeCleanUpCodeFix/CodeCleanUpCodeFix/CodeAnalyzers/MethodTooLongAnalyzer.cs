using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CodeCleanUpCodeFix.Consts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeCleanUpCodeFix.CodeAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodTooLongAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Method too long";
        private const string MessageFormat = "Method '{0}' has '{1}' lines which exceeds max number of lines: '{2}'.";
        private const string Description = "Method should be broken down to atomic level";

        private const int MaxLineNumberForMethod = 100;

        private static DiagnosticDescriptor TooLongMethodRule = new DiagnosticDescriptor(
            DiagnosticsConsts.MethodTooLongDiagnosticId,
            Title,
            MessageFormat,
            IssueCategoryConsts.DuplicateCode,
            DiagnosticSeverity.Warning,
            true,
            Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            TooLongMethodRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeClassDeclarationNodeForSameMethods, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClassDeclarationNodeForSameMethods(SyntaxNodeAnalysisContext context)
        {
            var currentClass = context.ContainingSymbol as INamedTypeSymbol;
            if (currentClass == null)
            {
                return;
            }

            if (currentClass.DeclaringSyntaxReferences.Length != 1)
            {
                return;
            }

            var currentClassSyntax = (ClassDeclarationSyntax)currentClass.DeclaringSyntaxReferences[0].GetSyntax();
            LookForMethodDuplicationWithinClass(context, currentClassSyntax);
        }

        private void LookForMethodDuplicationWithinClass(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax ownerClass)
        {
            var methods = ownerClass.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

            foreach (var method in methods)
            {
                var methodLineCount = method.Body.GetText().Lines.Count;
                if (methodLineCount > MaxLineNumberForMethod)
                {
                    var diagnostic = Diagnostic.Create(
                        TooLongMethodRule,
                        method.GetLocation(),
                        new List<Location>(),
                        method.Identifier.Value,
                        methodLineCount,
                        MaxLineNumberForMethod);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
