using System.Net.Http.Headers;
using System.Net.Mail;

namespace Application.UseCases.Wrapper
{
    public interface IWrapper
    {
        Task<string> ReadAsStringAsync(HttpContent content);
        Task<HttpResponseMessage> PostAsync(HttpClient client, string url, HttpContent? content, AuthenticationHeaderValue? authenticationHeaderValue);
        Task SendMailAsync(SmtpClient smtpClient, MailMessage message);
    }
}
