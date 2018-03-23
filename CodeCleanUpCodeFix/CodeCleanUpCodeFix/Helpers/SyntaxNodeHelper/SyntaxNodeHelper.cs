using System;
using System.Runtime.InteropServices;
using CodeCleanUpCodeFix.Helpers.WinApiMessage.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeCleanUpCodeFix.Helpers.WinApiMessage
{
    public class SyntaxNodeHelper
    {
        public static Location GetMethodBodyLocationFromDeclarationLocation(SyntaxNode root, Location declarationLocation)
        {
            var methodDeclarationSyntax = root.FindNode(declarationLocation.SourceSpan) as MethodDeclarationSyntax;
            if (methodDeclarationSyntax != null)
            {
                return methodDeclarationSyntax.Body.GetLocation();
            }
            return null;
        }

        public static SourceText GetSourceCodeFromLocation(SyntaxNode root, Location location)
        {
            var syntaxNode = root.FindNode(location.SourceSpan);
            return syntaxNode.GetText();
        }
    }
}
