namespace HMS.Ordering.API.Application.IntegrationEvents.EventHandling
{
    using Microsoft.BuildingBlocks.EventBus.Abstractions;
    using System.Threading.Tasks;
    using Events;
    using System.Linq;
    using HMS.Ordering.Domain.AggregatesModel.OrderAggregate;

    public class OrderStockRejectedIntegrationEventHandler : IIntegrationEventHandler<OrderStockRejectedIntegrationEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public OrderStockRejectedIntegrationEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Handle(OrderStockRejectedIntegrationEvent @event)
        {
            var orderToUpdate = await _orderRepository.GetAsync(@event.OrderId);

            var orderStockRejectedItems = @event.OrderStockItems
                .FindAll(c => !c.HasStock)
                .Select(c => c.ProductId);

            orderToUpdate.SetCancelledStatusWhenStockIsRejected(orderStockRejectedItems);

            await _orderRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }
}