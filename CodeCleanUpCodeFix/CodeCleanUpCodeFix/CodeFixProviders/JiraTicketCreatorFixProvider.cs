using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using CodeCleanUpCodeFix.CodeFixProviders.CodeActions;
using CodeCleanUpCodeFix.Consts;
using CodeCleanUpCodeFix.Helpers.WinApiMessage;

namespace CodeCleanUpCodeFix
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
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();

            var locations = new List<Location>
            {
                SyntaxNodeHelper.GetMethodBodyLocationFromDeclarationLocation(root, diagnostic.Location),
                SyntaxNodeHelper.GetMethodBodyLocationFromDeclarationLocation(root, diagnostic.AdditionalLocations.First())
            };

            var sourceCodeText = SyntaxNodeHelper.GetSourceCodeFromLocation(root, locations.First());

            var codeAction = new CreateJiraTicketCodeAction(
                context.Document,
                locations,
                sourceCodeText);

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private Task<Document> FixIssue()
        {
            // Must be somekind of RoslynAnalyzer bug
            // After removing this method CodeFixProviders are not picked up
            return null;
        }
    }
}
