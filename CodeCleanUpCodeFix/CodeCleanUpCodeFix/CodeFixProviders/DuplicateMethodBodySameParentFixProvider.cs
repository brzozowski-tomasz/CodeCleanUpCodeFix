using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using CodeCleanUpCodeFix.Consts;

namespace CodeCleanUpCodeFix
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
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            
            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Replace calling original method",
                    createChangedDocument: c => FixIssue(context.Document, declaration, c),
                    equivalenceKey: "Replace calling original method"),
                diagnostic);
        }

        private async Task<Document> FixIssue(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            var newMethod = methodDeclaration;
            newMethod = newMethod.WithBody(newMethod.Body.WithStatements(new SyntaxList<StatementSyntax>()));

            var newBody = "Method1(args);";
            newMethod = newMethod.AddBodyStatements(SyntaxFactory.ParseStatement(newBody));
            
            //SyntaxFactory.ParseStatement(newBody).WithLeadingTrivia(SyntaxFactory.Comment("SampleComment"));

            //// Get the symbol representing the type to be renamed.
            ////var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            //newMethod = newMethod.AddBodyStatements();

            var oldDocumentRootNode = document
                .GetSyntaxRootAsync(cancellationToken).Result
                .ReplaceNode(methodDeclaration, newMethod);

            var newDocumentRootNode = Formatter.Format(oldDocumentRootNode, new AdhocWorkspace());
            return document.WithSyntaxRoot(newDocumentRootNode);
        }
    }
}
