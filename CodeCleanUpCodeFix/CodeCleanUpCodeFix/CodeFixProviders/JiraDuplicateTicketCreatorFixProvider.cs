using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using CodeCleanUpCodeFix.CodeFixProviders.CodeActions;
using CodeCleanUpCodeFix.Consts;
using CodeCleanUpCodeFix.Helpers.JiraIntegration;
using CodeCleanUpCodeFix.Helpers.SyntaxHelpers;

namespace CodeCleanUpCodeFix.CodeFixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JiraTicketCreatorFixProvider)), Shared]
    public class JiraTicketCreatorFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticsConsts.DuplicateMethodBodySameParentDiagnosticId,
                    DiagnosticsConsts.DuplicatePropertySameBaseClassDiagnosticId);
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

            var locations = new List<Location>
            {
                MethodStatementHelper.GetMethodBodyLocationFromDeclarationLocation(root, diagnostic.Location),
                MethodStatementHelper.GetMethodBodyLocationFromDeclarationLocation(root, diagnostic.AdditionalLocations.First())
            };

            var sourceCodeText = SourceTextHelper.GetSourceCodeFromLocation(root, locations.First());

            var codeCleanUpTicketAction = new CreateJiraTicketCodeAction(
                "Create Code CleanUp Jira Ticket",
                TicketIssueType.DuplicateCode,
                context.Document,
                locations,
                sourceCodeText);

            var hutTicketAction = new CreateJiraTicketCodeAction(
                "Create HUT Jira Ticket",
                TicketIssueType.HandCraftedUnitTest,
                context.Document,
                locations,
                sourceCodeText);

            context.RegisterCodeFix(codeCleanUpTicketAction, diagnostic);
            context.RegisterCodeFix(hutTicketAction, diagnostic);
        }

        private Task<Document> FixIssue()
        {
            // Must be somekind of RoslynAnalyzer bug
            // After removing this method CodeFixProviders are not picked up
            return null;
        }
    }
}
