using HMS.Core.Models.User;
using System.Threading.Tasks;

namespace HMS.Core.Services.User
{
    public interface IUserService
    {
        Task<UserInfo> GetUserInfoAsync(string authToken);
    }
}
