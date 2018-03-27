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
using Microsoft.CodeAnalysis.Text;

namespace CodeCleanUpCodeFix.CodeFixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DuplicatePropertySameBaseClassFixProvider)), Shared]
    public class DuplicatePropertySameBaseClassFixProvider: CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
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

            

            var propertiesToElevate = new List<PropertyDeclarationSyntax>();
            propertiesToElevate.Add((PropertyDeclarationSyntax)root.FindNode(diagnostic.Location.SourceSpan));
            propertiesToElevate.Add((PropertyDeclarationSyntax)root.FindNode(diagnostic.AdditionalLocations.First().SourceSpan));

            var baseClassStart = int.Parse(diagnostic.Properties["BaseClassDeclarationStart"]);
            var baseClassLenght = int.Parse(diagnostic.Properties["BaseClassDeclarationLength"]);

            var targetBaseClass = (ClassDeclarationSyntax)root.FindNode(new TextSpan(baseClassStart, baseClassLenght));


            var codeAction = new ElevatePropertiesToClosestBaseClassCodeAction(
                root,
                context.Document,
                targetBaseClass,
                propertiesToElevate,
                context.CancellationToken);

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private Task<Document> FixIssue()
        {
            return null;
        }
    }
}
