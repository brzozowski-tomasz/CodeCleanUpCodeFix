using System;
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
using CodeCleanUpCodeFix.CodeFixProviders.CodeActions;
using CodeCleanUpCodeFix.Helpers.JiraIntegration;

namespace CodeCleanUpCodeFix
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DuplicateMethodBodySameParentFixProvider)), Shared]
    public class DuplicateMethodBodySameParentFixProvider : CodeFixProvider
    {
        private const string JiraUri = "https://jira.devfactory.com/rest/api/latest/";
        private const string JiraUser = "tbrzozowski";
        private const string JiraPassword = "Adkitdjcvqwirwrl234";

        private const string title = "Fix it";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CodeCleanUpCodeFixAnalyzer.DuplicateMethodBodySameParentDiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            //context.Span.
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => MakeUppercaseAsync(context.Document, declaration, c, false),
                    equivalenceKey: title),
                diagnostic);

            //var codeAction = new CreateJiraTicketCodeAction("Create Jira", c => MakeUppercaseAsync(context.Document, declaration, c, true), "Create Jira");

            ////CodeAction.Create(
            ////        title: "Create Jira",
            ////        createChangedDocument: c => MakeUppercaseAsync(context.Document, declaration, c, true),
            ////        equivalenceKey: "Create Jira");


            //context.RegisterCodeFix(codeAction,
            //    diagnostic);

            
            //context.Document.Project.Solution.Wo
        }

        private async Task<Document> MakeUppercaseAsync(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken, bool createJiraTicket)
        {
            

            var newMethod = methodDeclaration;
            newMethod = newMethod.WithBody(newMethod.Body.WithStatements(new SyntaxList<StatementSyntax>()));

            var newBody = "Method1(args);";

            if (createJiraTicket)
            {
                var jiraClient = new JiraClient(JiraUri, JiraUser, JiraPassword);
                var ticketToCreate = new Ticket
                {
                    ProjectCode = "CC",
                    Summary = "CodeFixTool_TicketCreatedByUnitTest_Summary",
                    Description = "CodeFixTool_TicketCreatedByUnitTest_Description",
                    IssueTypeName = "Duplicate Code"
                };

                var result = jiraClient.CreateTicket(ticketToCreate);
                //var result = new TicketCreationResult() {Self = "qwewqe"};
                newMethod = newMethod.AddBodyStatements(SyntaxFactory.EmptyStatement().WithLeadingTrivia(
                    SyntaxFactory.Comment("// Jira ticket " + result.Self)));
                Console.Out.WriteLine("// Jira ticket: some number linke whatever ");
                newMethod = newMethod.AddBodyStatements(SyntaxFactory.ParseStatement(newBody));
                
            }
            else
            {
                newMethod = newMethod.AddBodyStatements(SyntaxFactory.ParseStatement(newBody));

            }

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

        //private async Task<Document> CreateJiraTicket(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        //{
        //    //var jiraClient = new JiraClient(JiraUri, JiraUser, JiraPassword);
        //    //var ticketToCreate = new Ticket
        //    //{
        //    //    ProjectCode = "CC",
        //    //    Summary = "CodeFixTool_TicketCreatedByUnitTest_Summary",
        //    //    Description = "CodeFixTool_TicketCreatedByUnitTest_Description",
        //    //    IssueTypeName = "Duplicate Code"
        //    //};

        //    //var result = jiraClient.CreateTicket(ticketToCreate);

        //    var newBody = "/* CREATED TICKET: " + /*result.Self*/ 
        //         " /*" +
        //        "Method1(args);";

        //    // Get the symbol representing the type to be renamed.
        //    //var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

        //    methodDeclaration.Body.Update(
        //        methodDeclaration.Body.OpenBraceToken,
        //        new SyntaxList<StatementSyntax>(),
        //        methodDeclaration.Body.CloseBraceToken);

        //    var newMethod = methodDeclaration;
        //    newMethod = newMethod.WithBody(newMethod.Body.WithStatements(new SyntaxList<StatementSyntax>()));
        //    newMethod = newMethod.AddBodyStatements(SyntaxFactory.ParseStatement(newBody));

        //    var oldDocumentRootNode = document
        //        .GetSyntaxRootAsync(cancellationToken).Result
        //        .ReplaceNode(methodDeclaration, newMethod);

        //    var newDocumentRootNode = Formatter.Format(oldDocumentRootNode, new AdhocWorkspace());
        //    return document.WithSyntaxRoot(newDocumentRootNode);
        //}
    }
}
