using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;
using CodeCleanUpCodeFix.CodeAnalyzers;
using CodeCleanUpCodeFix.Consts;

namespace CodeCleanUpCodeFix.Test.CodeAnalyzers
{
    [TestClass]
    public class DuplicateMethodBodySameParentAnalyzerTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DuplicateMethodBodySameParentAnalyzer();
        }

        [TestMethod]
        public void DuplicateMethodBodySameParentAnalyzer_SourceCodeEmpty_NoDiagnosticIssuesRaised()
        {
            // Act, Assert
            VerifyCSharpDiagnostic(string.Empty);
        }

        [TestMethod]
        public void DuplicateMethodBodySameParentAnalyzer_EmptyClass_NoDiagnosticIssuesRaised()
        {
            // Act, Assert
            VerifyCSharpDiagnostic(CodeSnippetsConsts.EmptyClass);
        }

        [TestMethod]
        public void DuplicateMethodBodySameParentAnalyzer_ClassContainingEmptyMethod_NoDiagnosticIssuesRaised()
        {
            // Act, Assert
            VerifyCSharpDiagnostic(CodeSnippetsConsts.ClassContainingEmptyMethod);
        }

        [TestMethod]
        public void DuplicateMethodBodySameParentAnalyzer_ClassContainingTwoDifferentMethods_NoDiagnosticIssuesRaised()
        {
            // Act, Assert
            VerifyCSharpDiagnostic(CodeSnippetsConsts.ClassContainingTwoDifferentMethods);
        }

        [TestMethod]
        public void DuplicateMethodBodySameParentAnalyzer_ClassContainingTwoExactSameMethods_DiagnosticIssueRaised()
        {
            // Arrange
            var expectedIssueRaisedForFirstMethod = new DiagnosticResult
            {
                Id = DiagnosticsConsts.DuplicateMethodBodySameParentDiagnosticId,
                Message = "Method 'Method1' has exact same body as method 'Method2'.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 13, 13),
                        new DiagnosticResultLocation("Test0.cs", 22, 13)
                    },
                
            };

            var expectedIssueRaisedForSecondMethod = new DiagnosticResult
            {
                Id = DiagnosticsConsts.DuplicateMethodBodySameParentDiagnosticId,
                Message = "Method 'Method2' has exact same body as method 'Method1'.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 22, 13),
                        new DiagnosticResultLocation("Test0.cs", 13, 13),
                    },
            };

            // Act, Assert
            VerifyCSharpDiagnostic(
                CodeSnippetsConsts.ClassContainingTwoExactSameMethods,
                expectedIssueRaisedForFirstMethod,
                expectedIssueRaisedForSecondMethod
                );
        }
    }
}
