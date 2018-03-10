using Microsoft.BuildingBlocks.EventBus.Abstractions;
using HMS.Ordering.Domain.AggregatesModel.OrderAggregate;
using System.Threading.Tasks;
using HMS.IntegrationEvents;

namespace HMS.Ordering.API.Application.IntegrationEvents.EventHandling
{
    public class GracePeriodConfirmedIntegrationEventHandler : IIntegrationEventHandler<GracePeriodConfirmedIntegrationEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public GracePeriodConfirmedIntegrationEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// Event handler which confirms that the grace period
        /// has been completed and order will not initially be cancelled.
        /// Therefore, the order process continues for validation. 
        /// </summary>
        /// <param name="event">       
        /// </param>
        /// <returns></returns>
        public async Task Handle(GracePeriodConfirmedIntegrationEvent @event)
        {
			Order orderToUpdate = await _orderRepository.GetAsync(@event.OrderId);
            orderToUpdate.SetAwaitingValidationStatus();
            await _orderRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }
}
