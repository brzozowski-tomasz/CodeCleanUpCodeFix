using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using CodeCleanUpCodeFix.Helpers.JiraIntegration;

namespace JiraIntegration.Test
{
    [TestClass]
    public class JiraClientTest
    {
        private const string JiraUri = "https://jira.devfactory.com/rest/api/latest/";
        private const string JiraUser = "tbrzozowski";
        private const string JiraPassword = "Adkitdjcvqwirwrl234";

        [TestMethod]
        [Ignore]
        public void JiraClient_CreateTicket_CorrectTicketDataSent_TicketCreatedSuccessfully()
        {
            // Arrange
            var jiraClient = new JiraClient(JiraUri, JiraUser, JiraPassword);
            var ticketToCreate = new Ticket
            {
                ProjectCode = "CC",
                Summary = "CodeFixTool_TicketCreatedByUnitTest_Summary",
                Description = "CodeFixTool_TicketCreatedByUnitTest_Description",
                IssueTypeName = "Duplicate Code"
            };

            // Act
            var result = jiraClient.CreateTicket(ticketToCreate);

            // Assert
            result.IsSuccessfull.ShouldBeTrue();
            result.Id.ShouldNotBeNullOrEmpty();
            result.ResponseMessage.ShouldNotBeNullOrEmpty();
            result.Key.ShouldNotBeNullOrEmpty();
            result.Self.ShouldNotBeNullOrEmpty();
        }

        [TestMethod]
        public void JiraClient_CreateTicket_WrongTicketDataSent_TicketNotCreated()
        {
            // Arrange
            var jiraClient = new JiraClient(JiraUri, JiraUser, JiraPassword);
            var ticketToCreate = new Ticket
            {
                ProjectCode = "NotExistingProject",
                Summary = "SampleSummary",
                Description = "SampleDescription",
                IssueTypeName = "Duplicate Code"
            };

            // Act
            var result = jiraClient.CreateTicket(ticketToCreate);

            // Assert
            result.IsSuccessfull.ShouldBeFalse();
            result.Id.ShouldBeNull();
            result.ResponseMessage.ShouldNotBeNullOrEmpty();
            result.Key.ShouldBeNull();
            result.Self.ShouldBeNull();
        }
    }
}
