using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeCleanUpCodeFix.Helpers.SyntaxExtensions
{
    public static class SolutionExtensions
    {
        public static Document GetDocumentContainingNode(this Solution solution, SyntaxNode node)
        {
            var documentId = solution.GetDocumentIdsWithFilePath(node.SyntaxTree.FilePath).FirstOrDefault();

            if (documentId == null && solution.Projects.Count() == 1 && solution.Projects.First().Documents.Count() == 1)
            {
                //Fix for unit tests - documentFilePath is empty since test code snippets don't have filePath
                documentId = solution.Projects.First().Documents.First().Id;
            }
            if (documentId == null)
            {
                return null;
            }
            
            return solution.GetDocument(documentId);
        }
    }
}
