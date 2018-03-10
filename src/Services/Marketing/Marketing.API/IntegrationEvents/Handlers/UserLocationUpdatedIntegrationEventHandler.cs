using HMS.Marketing.API.Model;
using Microsoft.BuildingBlocks.EventBus.Abstractions;
using HMS.Marketing.API.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HMS.IntegrationEvents.Events;

namespace HMS.Marketing.API.IntegrationEvents.Handlers
{

    public class UserLocationUpdatedIntegrationEventHandler 
        : IIntegrationEventHandler<UserLocationUpdatedIntegrationEvent>
    {
        private readonly IMarketingDataRepository _marketingDataRepository;

        public UserLocationUpdatedIntegrationEventHandler(IMarketingDataRepository repository)
        {
            _marketingDataRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task Handle(UserLocationUpdatedIntegrationEvent @event)
        {
			MarketingData userMarketingData = await _marketingDataRepository.GetAsync(@event.UserId);
            userMarketingData = userMarketingData ?? 
                new MarketingData() { UserId = @event.UserId };

            userMarketingData.Locations = MapUpdatedUserLocations(@event.LocationList);
            await _marketingDataRepository.UpdateLocationAsync(userMarketingData);
        }

        private List<Location> MapUpdatedUserLocations(List<UserLocationDetails> newUserLocations)
        {
			List<Location> result = new List<Location>();
            newUserLocations.ForEach(location => {
                result.Add(new Location()
                {
                    LocationId = location.LocationId,
                    Code = location.Code,
                    Description = location.Description
                });
            });

            return result;
        }
    }
}
