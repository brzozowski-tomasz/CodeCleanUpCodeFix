using System;
using System.Net.Http;
using System.Text;
using CodeCleanUpCodeFix.Helpers.JiraIntegration.Interfaces;
using Newtonsoft.Json;

namespace CodeCleanUpCodeFix.Helpers.JiraIntegration
{
    public class JiraClient : IJiraClient
    {
        private string _uri;
        private string _user;
        private string _password;

        public JiraClient(string uri, string user, string password)
        {
            _uri = uri;
            _user = user;
            _password = password;
        }

        public TicketCreationResult CreateTicket(Ticket ticket)
        {
            var requestBody = GetCreateTicketRequestBody(ticket);
            var httpClient = CreateHttpClient();
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(JiraConsts.IssueEndpoint, content).Result;
            var responseMessage = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var result = new TicketCreationResult();
            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<TicketCreationResult>(responseMessage);
            }

            result.IsSuccessfull = response.IsSuccessStatusCode;
            result.ResponseMessage = responseMessage;
            return result;
        }

        private string GetCreateTicketRequestBody(Ticket ticket)
        {
            return string.Format(
                "{{\"fields\":{{\"project\":{{\"key\":\"{0}\"}},\"summary\":\"{1}\",\"description\":\"{2}\",\"issuetype\":{{\"name\":\"{3}\"}}}}}}",
                ticket.ProjectCode,
                ticket.Summary,
                ticket.Description,
                ticket.IssueTypeName);
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_uri)
            };

            var cred = Encoding.UTF8.GetBytes(_user + ":" + _password);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(cred));
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
