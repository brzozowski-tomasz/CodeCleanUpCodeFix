using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;
using CodeCleanUpCodeFix.CodeAnalyzers;
using CodeCleanUpCodeFix.Consts;

namespace CodeCleanUpCodeFix.Test.CodeAnalyzers
{
    [TestClass]
    public class ClassSimilarityAnalyzerTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ClassSimilarityAnalyzer();
        }

        [TestMethod]
        public void ClassSimilarityAnalyzer_SourceCodeEmpty_NoDiagnosticIssuesRaised()
        {
            // Act, Assert
            VerifyCSharpDiagnostic(string.Empty);
        }

        [TestMethod]
        public void ClassSimilarityAnalyzer_EmptyClass_NoDiagnosticIssuesRaised()
        {
            // Act, Assert
            VerifyCSharpDiagnostic(CodeSnippetsConsts.EmptyClass);
        }

        [TestMethod]
        public void ClassSimilarityAnalyzer_ClassesNotHavingEnoughSimilarity_NoDiagnosticIssuesRaised()
        {
            var testSourceCode = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class ClassA
        {
            private int _someField;

            public int SomeProperty
            {
                get
                {
                    return _someField;
                }
                set
                {
                    _someField = value;
                }
            }
        }

        class ClassB: BaseClass
        {
            private int _someField;

            public int SomeProperty
            {
                get
                {
                    return _someField;
                }
                set
                {
                    _someField = value;
                }
            }
        }
    }";

            // Act, Assert
            VerifyCSharpDiagnostic(testSourceCode);
        }

        [TestMethod]
        public void ClassSimilarityAnalyzer_ClassesHavingEnoughSimilarity_DiagnosticIssuesRaised()
        {
            // Arrange
            var testSourceCode = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class ClassA
        {
            private int _sameField1;
            protected string _sameField2;
            protected string _differentField3;

            public int SameProperty1
            {
                get
                {
                    return _someField;
                }
                set
                {
                    _someField = value;
                }
            }

            public string SameProperty2
            {
                get
                {
                    return _sameField2;
                }
                set
                {
                    _sameField2 = value;
                }
            }

            public void SameMethod1()
            {
                Console.WriteLine(string.Empty));
                Console.WriteLine(string.Empty));
                Console.WriteLine(string.Empty));
                Console.WriteLine(string.Empty));
                Console.WriteLine(string.Empty));
                Console.WriteLine(string.Empty));
            }
        }

        class ClassB
        {
            private int _sameField1;
            protected string _sameField2;

            public int SameProperty1
            {
                get
                {
                    return _someField;
                }
                set
                {
                    _someField = value;
                }
            }

            public string SameProperty2
            {
                get
                {
                    return _sameField2;
                }
                set
                {
                    _sameField2 = value;
                }
            }

            public void SameMethod1()
            {
                Console.WriteLine(string.Empty));
                Console.WriteLine(string.Empty));
                Console.WriteLine(string.Empty));
                Console.WriteLine(string.Empty));
                Console.WriteLine(string.Empty));
                Console.WriteLine(string.Empty));
            }
        }
    }";
            
            var expectedIssueRaisedForFirstClass = new DiagnosticResult
            {
                Id = DiagnosticsConsts.SimilarClassesDiagnosticId,
                Message = "Class 'ClassA' is similar to 'ClassB' - classes match by 2 fields, 2 properties and 1 methods (having 11 lines of code).",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 11, 9),
                        new DiagnosticResultLocation("Test0.cs", 52, 9)
                    }
            };

            var expectedIssueRaisedForSecondClass = new DiagnosticResult
            {
                Id = DiagnosticsConsts.SimilarClassesDiagnosticId,
                Message = "Class 'ClassB' is similar to 'ClassA' - classes match by 2 fields, 2 properties and 1 methods (having 11 lines of code).",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 52, 9),
                        new DiagnosticResultLocation("Test0.cs", 11, 9),
                    }
            };

            // Act, Assert
            VerifyCSharpDiagnostic(
                testSourceCode,
                expectedIssueRaisedForFirstClass,
                expectedIssueRaisedForSecondClass
                );
        }
    }
}
