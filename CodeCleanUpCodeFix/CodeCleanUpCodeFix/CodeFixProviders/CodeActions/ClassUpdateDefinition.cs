using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeCleanUpCodeFix.CodeFixProviders.CodeActions
{
    class ClassUpdateDefinition
    {
        public ClassDeclarationSyntax NewClassDeclarationSyntax { get; set; }

        public SyntaxList<UsingDirectiveSyntax> NewClassUsings { get; set; }
        
    }
}
