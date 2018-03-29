using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeCleanUpCodeFix.Helpers.SyntaxHelpers
{
    public static class CompilationHelper
    {
        public static List<T> GetDeclarationsFromCompilation<T>(Compilation compilation)
        {
            var allClasses = new List<T>();

            foreach (var tree in compilation.SyntaxTrees)
            {
                var root = tree.GetRootAsync().GetAwaiter().GetResult();
                allClasses.AddRange(root.DescendantNodes().OfType<T>());
            }
            return allClasses;
        }

    }
}
