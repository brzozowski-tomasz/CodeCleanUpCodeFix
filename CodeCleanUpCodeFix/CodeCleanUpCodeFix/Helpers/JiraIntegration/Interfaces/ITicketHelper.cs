using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CodeCleanUpCodeFix.Helpers.JiraIntegration.Interfaces
{
    public interface ITicketHelper
    {
        string GetDuplicateTicketSummary(Document document);

        string GetDuplicateTicketDescription(Document document, List<Location> locations, SourceText sourceText);
    }
}