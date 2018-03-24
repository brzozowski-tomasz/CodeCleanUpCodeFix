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
    public class DuplicateMethodBodySameParentAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString Title = "Duplicate method";
        private static readonly LocalizableString MessageFormat = "Method '{0}' has exact same body as method '{1}'.";
        private static readonly LocalizableString Description = "Duplicated code should be removed";

        private static DiagnosticDescriptor DuplicatedMethodBodyRule = new DiagnosticDescriptor(
            DiagnosticsConsts.DuplicateMethodBodySameParentDiagnosticId,
            Title,
            MessageFormat,
            IssueCategoryConsts.DuplicateCode,
            DiagnosticSeverity.Warning,
            true,
            Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            DuplicatedMethodBodyRule);

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
                if(GetEqualBodyMethods(method, methods, out var equalBodyMethods))
                {
                    var equalBodyMethod = equalBodyMethods.First();
                    var diagnostic = Diagnostic.Create(
                        DuplicatedMethodBodyRule,
                        method.GetLocation(),
                        new List<Location> { equalBodyMethod.GetLocation() },
                        method.Identifier.Value,
                        equalBodyMethod.Identifier.Value);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private bool GetEqualBodyMethods(
            MethodDeclarationSyntax method,
            List<MethodDeclarationSyntax> methods,
            out List<MethodDeclarationSyntax> equalBodyMethods)
        {
            equalBodyMethods = new List<MethodDeclarationSyntax>();

            foreach (var anotherMethod in methods)
            {
                if (anotherMethod != null && anotherMethod.Identifier.Value != method.Identifier.Value)
                {
                    // Very simple comparison of methods - could be implemented smarter:
                    // parameters can be named differently and method could still be equal
                    // should also include percentage of similary (exact, strong/weak match etc...)
                    if (anotherMethod.Body.ToString() == method.Body.ToString())
                    {
                        equalBodyMethods.Add(anotherMethod);
                    }
                }
            }

            return equalBodyMethods.Count > 0;
        }
    }
}
