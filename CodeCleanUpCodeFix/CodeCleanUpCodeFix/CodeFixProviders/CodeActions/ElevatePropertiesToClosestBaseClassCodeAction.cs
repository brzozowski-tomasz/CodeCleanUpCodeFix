using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeCleanUpCodeFix.CodeFixProviders.CodeActions
{
    class ElevatePropertiesToClosestBaseClassCodeAction : CodeAction
    {
        private readonly Func<CancellationToken, Task<Document>> _createChangedDocument;
        
        public override string Title { get; }

        public override string EquivalenceKey { get; }

        public ElevatePropertiesToClosestBaseClassCodeAction(
            SyntaxNode root,
            Document document,
            ClassDeclarationSyntax targetBaseClass,
            List<PropertyDeclarationSyntax> propertiesToElevate,
            CancellationToken cancellationToken)
        {
            _createChangedDocument = c => ElevatePropertiesToClosestBaseClass(
                root,
                document,
                targetBaseClass,
                propertiesToElevate,
                cancellationToken);
            Title = "Elevate property to base class";
            EquivalenceKey = "Elevate property to base class";
        }

        protected override Task<Document> GetChangedDocumentAsync(
            CancellationToken cancellationToken)
        {
            return _createChangedDocument(cancellationToken);
        }

        private Task<Document> ElevatePropertiesToClosestBaseClass(
            SyntaxNode root,
            Document document,
            ClassDeclarationSyntax targetBaseClass,
            List<PropertyDeclarationSyntax> propertiesToElevate,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromResult(document);
            }

            RemovePropertiesFromChildClasses(ref root, propertiesToElevate);
            AddPropertyToBaseClass(ref root, targetBaseClass, propertiesToElevate.First());

            return Task.FromResult(document.WithSyntaxRoot(root));
        }

        private void AddPropertyToBaseClass(
            ref SyntaxNode root,
            ClassDeclarationSyntax targetBaseClass,
            PropertyDeclarationSyntax propertyToAdd)
        {
            var targetBaseClassIdentifier = targetBaseClass.Identifier.ValueText;
            targetBaseClass = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(classNode => classNode.Identifier.ValueText == targetBaseClassIdentifier);

            var newTargetBaseClassMembers = targetBaseClass.Members.Add(propertyToAdd);
            var newtargetBaseClass = targetBaseClass.WithMembers(newTargetBaseClassMembers);

            root = root.ReplaceNode(targetBaseClass, newtargetBaseClass);
        }

        private void RemovePropertiesFromChildClasses(
            ref SyntaxNode root,
            List<PropertyDeclarationSyntax> propertiesToElevate)
        {
            var nodesToRemove = new List<SyntaxNode>();

            foreach (var propertyToElevate in propertiesToElevate)
            {
                var node = root.FindNode(propertyToElevate.FullSpan);
                nodesToRemove.Add(node);
            }

            root = root.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia);
        }
    }
}
