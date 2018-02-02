using System.Threading.Tasks;

namespace HMS.Identity.API.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
