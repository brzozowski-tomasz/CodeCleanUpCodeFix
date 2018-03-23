using System;
using System.Runtime.InteropServices;
using CodeCleanUpCodeFix.Helpers.WinApiMessage.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeCleanUpCodeFix.Helpers.SyntaxHelpers
{
    public class SourceTextHelper
    {
        public static SourceText GetSourceCodeFromLocation(SyntaxNode root, Location location)
        {
            var syntaxNode = root.FindNode(location.SourceSpan);
            return syntaxNode.GetText();
        }
    }
}
