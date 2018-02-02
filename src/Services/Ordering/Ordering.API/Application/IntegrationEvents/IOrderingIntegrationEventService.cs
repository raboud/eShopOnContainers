using Microsoft.BuildingBlocks.EventBus.Events;
using System.Threading.Tasks;

namespace HMS.Ordering.API.Application.IntegrationEvents
{
    public interface IOrderingIntegrationEventService
    {
        Task PublishThroughEventBusAsync(IntegrationEvent evt);
    }
}
