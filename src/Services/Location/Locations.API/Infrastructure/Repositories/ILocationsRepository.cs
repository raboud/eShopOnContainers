namespace HMS.Locations.API.Infrastructure.Repositories
{
    using HMS.Locations.API.Model;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ViewModel;

    public interface ILocationsRepository
    {        
        Task<Locations> GetAsync(int locationId);

        Task<List<Locations>> GetLocationListAsync();

        Task<UserLocation> GetUserLocationAsync(string userId);

        Task<List<Locations>> GetCurrentUserRegionsListAsync(LocationRequest currentPosition);

        Task AddUserLocationAsync(UserLocation location);

        Task UpdateUserLocationAsync(UserLocation userLocation);

    }
}
