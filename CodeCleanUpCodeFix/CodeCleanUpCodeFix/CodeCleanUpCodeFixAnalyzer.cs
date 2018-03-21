using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeCleanUpCodeFix
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CodeCleanUpCodeFixAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CodeCleanUpCodeFix";

        public const string DuplicateMethodBodySameParentDiagnosticId = "DuplicateMethodBodySameParent";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        private static DiagnosticDescriptor DuplicatedMethodBodyRule = new DiagnosticDescriptor(DuplicateMethodBodySameParentDiagnosticId, "Method body duplicated", "Method body duplicated format", Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule, DuplicatedMethodBodyRule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterCodeBlockAction(AnalyzeDuplicateMethodInSameClass);
        }

        private void AnalyzeDuplicateMethodInSameClass(CodeBlockAnalysisContext context)
        {
            var currentMethod = context.OwningSymbol as IMethodSymbol;
            if (currentMethod == null)
                return;

            if (currentMethod.DeclaringSyntaxReferences.Length != 1)
                return;

            var currentMethodSyntax = (MethodDeclarationSyntax) currentMethod.DeclaringSyntaxReferences[0].GetSyntax();

            if (currentMethod != null)
            {
                var parentClass = currentMethod.ContainingType;
                var members = parentClass.GetMembers();

                foreach (var member in members)
                {
                    var anotherMethod = member as IMethodSymbol;
                    if (anotherMethod != null && anotherMethod.Name != currentMethod.Name)
                    {
                        if (anotherMethod.DeclaringSyntaxReferences.Length == 1)
                        {
                            var anotherMethodSyntax = (MethodDeclarationSyntax) anotherMethod.DeclaringSyntaxReferences[0].GetSyntax();
                            if (anotherMethodSyntax.Body.ToString() == currentMethodSyntax.Body.ToString())
                            {
                                var diagnostic = Diagnostic.Create(DuplicatedMethodBodyRule, currentMethod.Locations[0], currentMethod.Name);

                                context.ReportDiagnostic(diagnostic);
                            }
                        }
                    }

                }
            }

        }

        private bool AreBodiesEqual(SyntaxNode currentMethodSyntax, SyntaxNode anotherMethodSyntax)
        {
            var currentChildNodes = currentMethodSyntax.ChildNodes().ToList();
            var anotherChildNodes = anotherMethodSyntax.ChildNodes().ToList();

            if (anotherChildNodes.Count != currentChildNodes.Count)
            {
                return false;
            }

            for(var i = 0; i< currentChildNodes.Count; i++)
            {
                if (currentChildNodes[i].ToString() != anotherChildNodes[i].ToString())
                {
                    return false;
                }
            }

            return true;
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
