using System.Threading.Tasks;
using HMS.WebMVC.Models;

namespace HMS.WebMVC.Services
{
    public interface ILocationService
    {
        Task CreateOrUpdateUserLocation(LocationDTO location);
    }
}
