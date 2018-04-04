using System.Collections.Generic;
using System.Linq;
using CodeCleanUpCodeFix.Helpers.SyntaxExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeCleanUpCodeFix.Helpers.SyntaxHelpers
{
    public class ClassDeclarationSyntaxHelper
    {
        public static bool CheckIfAllClassesHaveSameBaseClass(
            List<ClassDeclarationSyntax> classes)
        {
            var baseClassName = classes.First().GetBaseClassName();

            foreach (var classToCheckBaseClass in classes)
            {
                if (classToCheckBaseClass.GetBaseClassName() != baseClassName)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
