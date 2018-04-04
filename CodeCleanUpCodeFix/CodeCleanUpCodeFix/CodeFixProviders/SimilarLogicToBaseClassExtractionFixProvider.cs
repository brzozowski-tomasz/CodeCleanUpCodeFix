using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using CodeCleanUpCodeFix.CodeFixProviders.CodeActions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeCleanUpCodeFix.Consts;
using CodeCleanUpCodeFix.Helpers.SyntaxHelpers;

namespace CodeCleanUpCodeFix.CodeFixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SimilarLogicToBaseClassExtractionFixProvider)), Shared]
    public class SimilarLogicToBaseClassExtractionFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticsConsts.SimilarClassesDiagnosticId);
            }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            var classesContainingSimilarLogic = new List<ClassDeclarationSyntax>();
            classesContainingSimilarLogic.Add(GetClassDeclarationSyntax(diagnostic.Location));
            foreach (var location in diagnostic.AdditionalLocations)
            {
                classesContainingSimilarLogic.Add(GetClassDeclarationSyntax(location));
            }

           var codeAction = new ElevateSharedLogicToBaseClassCodeAction(
                context.Document,
                classesContainingSimilarLogic,
                context.CancellationToken);

            context.RegisterCodeFix(codeAction, diagnostic);
            return Task.FromResult(true);
        }

        private ClassDeclarationSyntax GetClassDeclarationSyntax(Location location)
        {
            var root = location.SourceTree.GetRoot();
            return (ClassDeclarationSyntax)root.FindNode(location.SourceSpan);
        }

        private Task<Document> FixIssue()
        {
            return null;
        }
    }
}
