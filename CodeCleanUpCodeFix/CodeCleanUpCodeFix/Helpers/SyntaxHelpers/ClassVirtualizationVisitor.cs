using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeCleanUpCodeFix.Helpers.SyntaxHelpers
{
    public class ClassVirtualizationVisitor : CSharpSyntaxRewriter
    {
        public List<ClassDeclarationSyntax> Classes = new List<ClassDeclarationSyntax>();

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

            Classes.Add(node);

            return node;
        }
    }
}
