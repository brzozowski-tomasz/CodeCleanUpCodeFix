using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;
using CodeCleanUpCodeFix.CodeAnalyzers;
using CodeCleanUpCodeFix.Consts;

namespace CodeCleanUpCodeFix.Test.CodeAnalyzers
{
    [TestClass]
    public class MethodTooLongAnalyzerTest : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MethodTooLongAnalyzer();
        }

        [TestMethod]
        public void MethodTooLongAnalyzer_SourceCodeEmpty_NoDiagnosticIssuesRaised()
        {
            // Act, Assert
            VerifyCSharpDiagnostic(string.Empty);
        }

        [TestMethod]
        public void MethodTooLongAnalyzer_EmptyClass_NoDiagnosticIssuesRaised()
        {
            // Act, Assert
            VerifyCSharpDiagnostic(CodeSnippetsConsts.EmptyClass);
        }

        [TestMethod]
        public void MethodTooLongAnalyzer_ClassContainingEmptyMethod_NoDiagnosticIssuesRaised()
        {
            // Act, Assert
            VerifyCSharpDiagnostic(CodeSnippetsConsts.ClassContainingEmptyMethod);
        }

        [TestMethod]
        public void MethodTooLongAnalyzer_ClassContainingShortMethod_NoDiagnosticIssuesRaised()
        {
            // Act, Assert
            VerifyCSharpDiagnostic(CodeSnippetsConsts.ClassContainingShortMethod);
        }

        [TestMethod]
        public void MethodTooLongAnalyzer_ClassContainingLongMethod_DiagnosticIssueRaised()
        {
            // Arrange
            var expectedIssueRaised = new DiagnosticResult
            {
                Id = DiagnosticsConsts.MethodTooLongDiagnosticId,
                Message = "Method 'LongMethod' has '101' lines which exceeds max number of lines: '100'.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 13),
                        }
            };

            // Act, Assert
            VerifyCSharpDiagnostic(CodeSnippetsConsts.ClassContainingLongMethod, expectedIssueRaised);
        }
    }
}
