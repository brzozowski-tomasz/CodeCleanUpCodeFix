using System;
using System.Collections.Generic;
using System.Text;

namespace CodeCleanUpCodeFix.Helpers.JiraIntegration
{
    public class TicketDescriptionLocation
    {
        public int BeginLine { get; set; }
        public int EndLine { get; set; }
        public int CountLineCode { get; set; }
        public string Pkg { get; set; }
        public string RelFile { get; set; }
    }
}
