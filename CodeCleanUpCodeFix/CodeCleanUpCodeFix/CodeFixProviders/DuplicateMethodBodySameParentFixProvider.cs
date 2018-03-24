using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using CodeCleanUpCodeFix.CodeFixProviders.CodeActions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeCleanUpCodeFix.Consts;

namespace CodeCleanUpCodeFix.CodeFixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DuplicateMethodBodySameParentFixProvider)), Shared]
    public class DuplicateMethodBodySameParentFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticsConsts.DuplicateMethodBodySameParentDiagnosticId);
            }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var locationWithEqualMethod = diagnostic.AdditionalLocations.First();

            var methodToModify = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            var codeAction = new ReplaceMethodBodyWithEqualMethodInvocationCodeAction(
                root,
                context.Document,
                methodToModify,
                locationWithEqualMethod,
                context.CancellationToken);

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private Task<Document> FixIssue()
        {
            return null;
        }
    }
}
