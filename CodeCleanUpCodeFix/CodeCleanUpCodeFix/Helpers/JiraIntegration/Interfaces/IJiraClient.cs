namespace CodeCleanUpCodeFix.Helpers.JiraIntegration.Interfaces
{
    public interface IJiraClient
    {
        TicketCreationResult CreateTicket(Ticket ticket);
    }
}