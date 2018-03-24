using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using TestHelper;
using CodeCleanUpCodeFix;
using CodeCleanUpCodeFix.CodeAnalyzers;
using CodeCleanUpCodeFix.CodeFixProviders;
using JiraIntegration;

namespace CodeCleanUpCodeFix.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }



        public static string RunQuery(string query, string argument = null, string data = null, string method = "GET")
        {
            try
            {
                var m_BaseUrl = "https://jira.devfactory.com/rest/api/latest/issue/CC-29219";
                HttpWebRequest newRequest = WebRequest.Create(m_BaseUrl) as HttpWebRequest;
                newRequest.ContentType = "application/json";
                newRequest.Method = method;

                if (data != null)
                {
                    using (StreamWriter writer = new StreamWriter(newRequest.GetRequestStream()))
                    {
                        writer.Write(data);
                    }
                }

                string base64Credentials = GetEncodedCredentials();
                newRequest.Headers.Add("Authorization", "Basic " + base64Credentials);

                HttpWebResponse response = newRequest.GetResponse() as HttpWebResponse;

                string result = string.Empty;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }

                newRequest = null;
                response = null;

                return result;
            }
            catch (Exception)
            {
                //MessageBox.Show(@"There is a problem getting data from Jira :" + "\n\n" + query, "Jira Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private static string GetEncodedCredentials()
        {
            string mergedCredentials = string.Format("{0}:{1}", "tbrzozowski", "Adkitdjcvqwirwrl234");
            byte[] byteCredentials = UTF8Encoding.UTF8.GetBytes(mergedCredentials);
            return Convert.ToBase64String(byteCredentials);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "CodeCleanUpCodeFix",
                Message = String.Format("Type name '{0}' contains lowercase letters", "TypeName"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new DuplicateMethodBodySameParentFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DuplicateMethodBodySameParentAnalyzer();
        }
    }
}
