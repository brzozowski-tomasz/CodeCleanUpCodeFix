using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeCleanUpCodeFix.Helpers.SyntaxExtensions
{
    public static class ClassDeclarationSyntaxExtensions
    {
        public static bool IsInheritedFrom(this ClassDeclarationSyntax childClass, ClassDeclarationSyntax baseClass)
        {
            if (childClass.BaseList != null)
            {
                foreach (var baseType in childClass.BaseList.Types.OfType<SimpleBaseTypeSyntax>())
                {
                    var baseTypeIdentifier = baseType.Type as IdentifierNameSyntax;
                    if (baseTypeIdentifier != null)
                    {
                        if(baseTypeIdentifier.Identifier.ValueText == baseClass.Identifier.ValueText)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static string GetBaseClassName(this ClassDeclarationSyntax childClass)
        {
            if (childClass.BaseList != null)
            {
                var baseType = childClass.BaseList.Types.OfType<SimpleBaseTypeSyntax>().FirstOrDefault();
                if (baseType != null)
                {
                    var baseTypeIdentifier = baseType.Type as IdentifierNameSyntax;
                    if (baseTypeIdentifier != null)
                    {
                        return baseTypeIdentifier.Identifier.ValueText;
                    }
                }
            }

            return string.Empty;
        }
    }
}
