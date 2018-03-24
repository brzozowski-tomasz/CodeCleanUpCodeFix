using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeCleanUpCodeFix.Helpers.JiraIntegration;
using CodeCleanUpCodeFix.Helpers.JiraIntegration.Interfaces;
using CodeCleanUpCodeFix.Helpers.WinApiMessage;
using CodeCleanUpCodeFix.Helpers.WinApiMessage.Interfaces;
using Microsoft.CodeAnalysis.Text;

namespace CodeCleanUpCodeFix.CodeFixProviders.CodeActions
{
    class CreateJiraTicketCodeAction: CodeAction
    {
        private IJiraClient _jiraClient;
        private ITicketHelper _ticketHelper;
        private IWinApiMessageBox _winApiMessageBox;
        private readonly Func<CancellationToken, Task<Document>> _createChangedDocument;

        public IJiraClient JiraClient
        {
            get
            {
                if (_jiraClient == null)
                {
                    _jiraClient = new JiraClient(
                        GlobalSettingsConsts.JiraUri,
                        GlobalSettingsConsts.JiraUser,
                        GlobalSettingsConsts.JiraPassword);
                }

                return _jiraClient;
            }
            set
            {
                _jiraClient = value;
            }
        }

        public ITicketHelper TicketHelper
        {
            get
            {
                if (_ticketHelper == null)
                {
                    _ticketHelper = new TicketHelper();
                }

                return _ticketHelper;
            }
            set
            {
                _ticketHelper = value;
            }
        }

        public IWinApiMessageBox MessageBox
        {
            get
            {
                if (_winApiMessageBox == null)
                {
                    _winApiMessageBox = new WinApiMessageBox();
                }

                return _winApiMessageBox;
            }
            set
            {
                _winApiMessageBox = value;
            }
        }
        

        public override string Title { get; }

        public override string EquivalenceKey { get; }

        public CreateJiraTicketCodeAction(
            string title,
            TicketIssueType ticketType,
            Document document,
            List<Location> locations,
            SourceText sourceText)
        {
            _createChangedDocument = c => CreateJiraTicket(ticketType, document, locations, sourceText);
            Title = title;
            EquivalenceKey = title;
        }

        protected override Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Enumerable.Empty<CodeActionOperation>());
        }

        protected override Task<Document> GetChangedDocumentAsync(
            CancellationToken cancellationToken)
        {
            return _createChangedDocument(cancellationToken);
        }

        private async Task<Document> CreateJiraTicket(
            TicketIssueType ticketType,
            Document document,
            List<Location> locations,
            SourceText sourceText)
        {
            var ticketToCreate = new Ticket
            {
                ProjectCode = GlobalSettingsConsts.JiraProject,
                Summary = TicketHelper.GetDuplicateTicketSummary(document),
                Description = TicketHelper.GetDuplicateTicketDescription(document, locations, sourceText),
                IssueTypeName = MapTicketTypeToIssueTypeName(ticketType)
            };

            var result = JiraClient.CreateTicket(ticketToCreate);
            if (result.IsSuccessfull)
            {
                MessageBox.Show("Jira related ticket:", result.Self);
            }
            else
            {
                MessageBox.Show("Jira ticket creation failed:", result.ResponseMessage);
            }

            return document;
        }

        private string MapTicketTypeToIssueTypeName(TicketIssueType ticketType)
        {
            switch (ticketType)
            {
                case TicketIssueType.DuplicateCode: return JiraConsts.DuplicateCodeTicketType;
                case TicketIssueType.LongMethod: return JiraConsts.LongMethodTicketType;
                case TicketIssueType.HandCraftedUnitTest: return JiraConsts.HandCraftedUnitTestsTicketType;
            }

            return string.Empty;
        }
    }
}
