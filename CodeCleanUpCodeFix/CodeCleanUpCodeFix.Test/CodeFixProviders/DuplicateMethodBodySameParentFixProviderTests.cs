using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;
using CodeCleanUpCodeFix.CodeAnalyzers;
using CodeCleanUpCodeFix.CodeFixProviders;
using CodeCleanUpCodeFix.Consts;

namespace CodeCleanUpCodeFix.Test
{
    [TestClass]
    public class DuplicateMethodBodySameParentFixProviderTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DuplicateMethodBodySameParentAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new DuplicateMethodBodySameParentFixProvider();
        }

        [TestMethod]
        public void DuplicateMethodBodySameParentFixProvider_MethodsWithoutParameters_CodeFixedCorrectly()
        {
            // Arrange
            var expectedOutput = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class EmptyClass
    {
        private void Method1()
        {
            this.Method2();
        }

        private void Method2()
        {
            Console.WriteLine(String.Empty);
            Console.WriteLine(String.Empty);
            Console.WriteLine(String.Empty);
            Console.WriteLine(String.Empty);
            Console.WriteLine(String.Empty);
        }
    }
}";

            // Act, Assert
            VerifyCSharpFix(CodeSnippetsConsts.ClassContainingTwoExactSameMethods, expectedOutput);
        }
    }
}
