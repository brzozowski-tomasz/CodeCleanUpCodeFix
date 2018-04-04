using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;
using CodeCleanUpCodeFix.CodeAnalyzers;
using CodeCleanUpCodeFix.CodeFixProviders;

namespace CodeCleanUpCodeFix.Test
{
    [TestClass]
    public class DuplicatePropertySameBaseClassFixProviderTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DuplicatePropertySameBaseClassAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new DuplicatePropertySameBaseClassFixProvider();
        }

        [TestMethod]
        public void DuplicatePropertySameBaseClassFixProvider_MethodsWithoutParameters_CodeFixedCorrectly()
        {
            // Arrange
            var input = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class BaseClass
        {
        }

        class ChildClassA: BaseClass
        {
            private int _someField;

            public int SomeProperty
            {
                get; set;
            }
        }

        class ChildClassB: BaseClass
        {
            private int _someField;

            public int SomeProperty
            {
                get; set;
            }
        }
    }";
            var expectedOutput = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class BaseClass
        {

            public int SomeProperty
            {
                get; set;
            }
        }

        class ChildClassA: BaseClass
        {
            private int _someField;
        }

        class ChildClassB: BaseClass
        {
            private int _someField;
        }
    }";

            // Act, Assert
            VerifyCSharpFix(input, expectedOutput);
        }
    }
}
