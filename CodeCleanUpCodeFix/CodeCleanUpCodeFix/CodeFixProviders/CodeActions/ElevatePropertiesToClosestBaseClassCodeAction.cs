using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeCleanUpCodeFix.Helpers.SyntaxExtensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeCleanUpCodeFix.CodeFixProviders.CodeActions
{
    class ElevatePropertiesToClosestBaseClassCodeAction : CodeAction
    {
        private readonly Func<CancellationToken, Task<Solution>> _createChangedSolution;

        public override string Title { get; }

        public override string EquivalenceKey { get; }

        public ElevatePropertiesToClosestBaseClassCodeAction(
            Document document,
            ClassDeclarationSyntax targetBaseClass,
            List<PropertyDeclarationSyntax> propertiesToElevate,
            CancellationToken cancellationToken)
        {
            _createChangedSolution = c => ElevatePropertiesToClosestBaseClassSolution(
                document,
                targetBaseClass,
                propertiesToElevate,
                cancellationToken);

            Title = "Elevate property to base class";
            EquivalenceKey = "Elevate property to base class";
        }

        protected override Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
        {
            return _createChangedSolution(cancellationToken);
        }

        private Task<Solution> ElevatePropertiesToClosestBaseClassSolution(
            Document document,
            ClassDeclarationSyntax targetBaseClass,
            List<PropertyDeclarationSyntax> propertiesToElevate,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromResult(document.Project.Solution);
            }

            var solution = document.Project.Solution;
            
            RemovePropertiesFromChildClasses(ref solution, propertiesToElevate);
            AddPropertyToBaseClass(ref solution, targetBaseClass, propertiesToElevate.First());

            return Task.FromResult(solution);
        }

        private void AddPropertyToBaseClass(
            ref Solution solution,
            ClassDeclarationSyntax targetBaseClass,
            PropertyDeclarationSyntax propertyToAdd)
        {
            var newTargetBaseClassMembers = targetBaseClass.Members.Add(propertyToAdd);
            var newtargetBaseClass = targetBaseClass.WithMembers(newTargetBaseClassMembers);

            var document = solution.GetDocumentContainingNode(targetBaseClass);
            if (document == null)
            {
                return;
            }

            var documentRoot = document.GetSyntaxRootAsync().GetAwaiter().GetResult();

            var oldBaseClass = documentRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault(classDeclaration =>
                classDeclaration.Identifier.ValueText == targetBaseClass.Identifier.ValueText);

            var root = documentRoot.ReplaceNode(oldBaseClass, newtargetBaseClass);
            solution = solution.WithDocumentSyntaxRoot(document.Id, root);
        }

        private void RemovePropertiesFromChildClasses(
            ref Solution solution,
            List<PropertyDeclarationSyntax> propertiesToElevate)
        {
            var treesToModifyDictionary = new Dictionary<SyntaxTree, List<SyntaxNode>>();

            foreach (var propertyToElevate in propertiesToElevate)
            {
                if (!treesToModifyDictionary.ContainsKey(propertyToElevate.SyntaxTree))
                {
                    treesToModifyDictionary.Add(propertyToElevate.SyntaxTree, new List<SyntaxNode>());
                }

                treesToModifyDictionary[propertyToElevate.SyntaxTree].Add(propertyToElevate);
            }

            foreach (var treeToModify in treesToModifyDictionary)
            {
                var originalRoot = treeToModify.Key.GetRoot();
                var root = originalRoot.RemoveNodes(treeToModify.Value, SyntaxRemoveOptions.KeepNoTrivia);
                solution = solution.WithDocumentSyntaxRoot(solution.GetDocumentId(treeToModify.Key), root);
            }
        }
    }
}

