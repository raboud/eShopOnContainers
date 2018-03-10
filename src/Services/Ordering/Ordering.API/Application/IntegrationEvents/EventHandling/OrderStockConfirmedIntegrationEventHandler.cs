using Microsoft.BuildingBlocks.EventBus.Abstractions;
using System.Threading.Tasks;
using HMS.Ordering.Domain.AggregatesModel.OrderAggregate;
using HMS.IntegrationEvents.Events;

namespace HMS.Ordering.API.Application.IntegrationEvents.EventHandling
{
    public class OrderStockConfirmedIntegrationEventHandler : 
        IIntegrationEventHandler<OrderStockConfirmedIntegrationEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public OrderStockConfirmedIntegrationEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Handle(OrderStockConfirmedIntegrationEvent @event)
        {
			Order orderToUpdate = await _orderRepository.GetAsync(@event.OrderId);

            orderToUpdate.SetStockConfirmedStatus();

            await _orderRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }
}