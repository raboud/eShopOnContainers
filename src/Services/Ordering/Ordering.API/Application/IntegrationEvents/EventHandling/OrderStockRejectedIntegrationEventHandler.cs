using Microsoft.BuildingBlocks.EventBus.Abstractions;
using System.Threading.Tasks;
using System.Linq;
using HMS.Ordering.Domain.AggregatesModel.OrderAggregate;
using HMS.IntegrationEvents.Events;

namespace HMS.Ordering.API.Application.IntegrationEvents.EventHandling
{

	public class OrderStockRejectedIntegrationEventHandler : IIntegrationEventHandler<OrderStockRejectedIntegrationEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public OrderStockRejectedIntegrationEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Handle(OrderStockRejectedIntegrationEvent @event)
        {
			Order orderToUpdate = await _orderRepository.GetAsync(@event.OrderId);

			System.Collections.Generic.IEnumerable<int> orderStockRejectedItems = @event.OrderStockItems
                .FindAll(c => !c.HasStock)
                .Select(c => c.ProductId);

            orderToUpdate.SetCancelledStatusWhenStockIsRejected(orderStockRejectedItems);

            await _orderRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }
}