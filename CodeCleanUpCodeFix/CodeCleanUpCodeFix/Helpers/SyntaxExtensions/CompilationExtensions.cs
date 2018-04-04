using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeCleanUpCodeFix.Helpers.SyntaxHelpers
{
    public static class CompilationExtensions
    {
        public static List<T> DescendantNodes<T>(this Compilation compilation)
        {
            var descendantNodes = new List<T>();

            foreach (var tree in compilation.SyntaxTrees)
            {
                var root = tree.GetRootAsync().GetAwaiter().GetResult();
                descendantNodes.AddRange(root.DescendantNodes().OfType<T>());
            }
            return descendantNodes;
        }

    }
}
