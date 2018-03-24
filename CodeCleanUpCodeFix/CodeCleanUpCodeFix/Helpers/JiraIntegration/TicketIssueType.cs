using System.ComponentModel;

namespace CodeCleanUpCodeFix.Helpers.JiraIntegration
{
    public enum TicketIssueType
    {
        [Description(JiraConsts.DuplicateCodeTicketType)]
        DuplicateCode,

        [Description(JiraConsts.LongMethodTicketType)]
        LongMethod,

        [Description(JiraConsts.LongMethodTicketType)]
        HandCraftedUnitTest
    }
}
