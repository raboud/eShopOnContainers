using Microsoft.BuildingBlocks.EventBus.Abstractions;
using HMS.Ordering.Domain.AggregatesModel.OrderAggregate;
using System.Threading.Tasks;
using HMS.IntegrationEvents.Events;

namespace HMS.Ordering.API.Application.IntegrationEvents.EventHandling
{
    public class OrderPaymentFailedIntegrationEventHandler : 
        IIntegrationEventHandler<OrderPaymentFailedIntegrationEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public OrderPaymentFailedIntegrationEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Handle(OrderPaymentFailedIntegrationEvent @event)
        {
			Order orderToUpdate = await _orderRepository.GetAsync(@event.OrderId);

            orderToUpdate.SetCancelledStatus();

            await _orderRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }
}
