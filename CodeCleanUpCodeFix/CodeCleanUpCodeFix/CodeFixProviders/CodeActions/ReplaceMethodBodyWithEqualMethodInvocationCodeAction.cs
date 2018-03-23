using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System;
using System.Threading;
using System.Threading.Tasks;
using CodeCleanUpCodeFix.Helpers.SyntaxHelpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CodeCleanUpCodeFix.CodeFixProviders.CodeActions
{
    class ReplaceMethodBodyWithEqualMethodInvocationCodeAction : CodeAction
    {
        private readonly Func<CancellationToken, Task<Document>> _createChangedDocument;
        
        public override string Title { get; }

        public override string EquivalenceKey { get; }

        public ReplaceMethodBodyWithEqualMethodInvocationCodeAction(
            SyntaxNode root,
            Document document,
            MethodDeclarationSyntax methodToModify,
            Location locationWithEqualMethod,
            CancellationToken cancellationToken)
        {
            _createChangedDocument = c => ReplaceMethodBodyWithEqualMethodInvocation(
                root,
                document,
                methodToModify,
                locationWithEqualMethod,
                cancellationToken);
            Title = "Replace body by invoking equal method";
            EquivalenceKey = "Replace body by invoking equal method";
        }

        protected override Task<Document> GetChangedDocumentAsync(
            CancellationToken cancellationToken)
        {
            return _createChangedDocument(cancellationToken);
        }

        private Task<Document> ReplaceMethodBodyWithEqualMethodInvocation(
            SyntaxNode root,
            Document document,
            MethodDeclarationSyntax methodToModify,
            Location locationWithEqualMethod,
            CancellationToken cancellationToken)
        {
            var originalMethod = root.FindNode(locationWithEqualMethod.SourceSpan) as MethodDeclarationSyntax;
            if (originalMethod == null)
            {
                return Task.FromResult(document);
            }

            var modifiedMethod = MethodStatementHelper.GetMethodDeclarationCloneWithEmptyBody(methodToModify);
            var argumentList = MethodStatementHelper.GetInvocationParameterList(methodToModify.ParameterList.Parameters);

            var invocationExpressionStatement = MethodStatementHelper.GetMethodInvocationStatementSyntax(
                "this",
                originalMethod.Identifier.Text,
                argumentList);

            modifiedMethod = modifiedMethod.AddBodyStatements(invocationExpressionStatement);

            var modifiedDocumentRootNode = document
                .GetSyntaxRootAsync(cancellationToken)
                .Result
                .ReplaceNode(methodToModify, modifiedMethod);

            modifiedDocumentRootNode = Formatter.Format(modifiedDocumentRootNode, new AdhocWorkspace());
            return Task.FromResult(document.WithSyntaxRoot(modifiedDocumentRootNode));
        }
    }
}
