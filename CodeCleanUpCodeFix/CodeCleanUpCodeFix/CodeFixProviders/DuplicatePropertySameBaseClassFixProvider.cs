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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DuplicatePropertySameBaseClassFixProvider)), Shared]
    public class DuplicatePropertySameBaseClassFixProvider : CodeFixProvider
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

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            var propertiesToElevate = new List<PropertyDeclarationSyntax>
            {
                getPropertyDeclarationSyntax(diagnostic.Location),
                getPropertyDeclarationSyntax(diagnostic.AdditionalLocations.First())
            };

            var parentClass = propertiesToElevate.First().Parent as ClassDeclarationSyntax;
            if (parentClass == null || parentClass.BaseList == null)
            {
                return Task.FromResult(true);
            }

            var baseClassName = (parentClass.BaseList.Types.OfType<SimpleBaseTypeSyntax>().First().Type as IdentifierNameSyntax).Identifier.ValueText;
            var compilation = context.Document.Project.GetCompilationAsync().GetAwaiter().GetResult();
            var targetBaseClass = CompilationHelper.GetDeclarationsFromCompilation<ClassDeclarationSyntax>(compilation).First(classDeclaration =>
                classDeclaration.Identifier.ValueText == baseClassName);

            var codeAction = new ElevatePropertiesToClosestBaseClassCodeAction(
                context.Document,
                targetBaseClass,
                propertiesToElevate,
                context.CancellationToken);

            context.RegisterCodeFix(codeAction, diagnostic);
            return Task.FromResult(true);
        }

        private PropertyDeclarationSyntax getPropertyDeclarationSyntax(Location location)
        {
            var root = location.SourceTree.GetRoot();
            return (PropertyDeclarationSyntax)root.FindNode(location.SourceSpan);
        }

        private Task<Document> FixIssue()
        {
            return null;
        }
    }
}
