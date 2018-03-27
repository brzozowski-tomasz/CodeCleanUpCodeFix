using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;
using CodeCleanUpCodeFix.CodeAnalyzers;
using CodeCleanUpCodeFix.Consts;

namespace CodeCleanUpCodeFix.Test.CodeAnalyzers
{
    [TestClass]
    public class DuplicatePropertySameBaseClassAnalyzerTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DuplicatePropertySameBaseClassAnalyzer();
        }

        [TestMethod]
        public void DuplicatePropertySameBaseClassAnalyzer_SourceCodeEmpty_NoDiagnosticIssuesRaised()
        {
            // Act, Assert
            VerifyCSharpDiagnostic(string.Empty);
        }

        [TestMethod]
        public void DuplicatePropertySameBaseClassAnalyzer_EmptyClass_NoDiagnosticIssuesRaised()
        {
            // Act, Assert
            VerifyCSharpDiagnostic(CodeSnippetsConsts.EmptyClass);
        }

        [TestMethod]
        public void DuplicatePropertySameBaseClassAnalyzer_ClassesNotHavingSameBaseClassContainingSameProperty_NoDiagnosticIssuesRaised()
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
        class BaseClass
        {
        }

        class ChildClassA
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

        class ChildClassB: BaseClass
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
        public void DuplicatePropertySameBaseClassAnalyzer_ClassesHavingSameBaseClassNotContainingSameProperty_NoDiagnosticIssuesRaised()
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
        class BaseClass
        {
        }

        class ChildClassA: BaseClass
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

        class ChildClassB: BaseClass
        {
            public string SomeProperty
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
        public void DuplicatePropertySameBaseClassAnalyzer_ClassesHavingSameBaseClassContainingSameProperty_DiagnosticIssuesRaised()
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
        class BaseClass
        {
        }

        class ChildClassA: BaseClass
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

        class ChildClassB: BaseClass
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

        var expectedIssueRaisedForFirstMethod = new DiagnosticResult
            {
                Id = DiagnosticsConsts.DuplicatePropertySameBaseClassDiagnosticId,
                Message = "Property 'SomeProperty' in 'ChildClassA' class is exactly same as in 'ChildClassB' class.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 19, 13),
                        new DiagnosticResultLocation("Test0.cs", 36, 13)
                    }
            };

            var expectedIssueRaisedForSecondMethod = new DiagnosticResult
            {
                Id = DiagnosticsConsts.DuplicatePropertySameBaseClassDiagnosticId,
                Message = "Property 'SomeProperty' in 'ChildClassB' class is exactly same as in 'ChildClassA' class.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 36, 13),
                        new DiagnosticResultLocation("Test0.cs", 19, 13),
                    }
            };

            // Act, Assert
            VerifyCSharpDiagnostic(
                testSourceCode,
                expectedIssueRaisedForFirstMethod,
                expectedIssueRaisedForSecondMethod
                );
        }
    }
}
