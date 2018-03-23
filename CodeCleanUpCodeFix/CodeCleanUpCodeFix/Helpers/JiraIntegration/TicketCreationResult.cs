namespace CodeCleanUpCodeFix.Helpers.JiraIntegration
{
    public class TicketCreationResult
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Self { get; set; }
        public bool IsSuccessfull { get; set; }
        public string ResponseMessage { get; set; }
    }
}
