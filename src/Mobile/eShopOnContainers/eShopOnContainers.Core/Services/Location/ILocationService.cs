using System.Threading.Tasks;

namespace HMS.Core.Services.Location
{    
    public interface ILocationService
    {
        Task UpdateUserLocation(HMS.Core.Models.Location.Location newLocReq, string token);
    }
}