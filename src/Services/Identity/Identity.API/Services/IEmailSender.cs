using System.Threading.Tasks;

namespace HMS.Identity.API.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
