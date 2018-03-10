using Microsoft.BuildingBlocks.EventBus.Abstractions;
using HMS.Ordering.Domain.AggregatesModel.OrderAggregate;
using System.Threading.Tasks;
using HMS.IntegrationEvents.Events;

namespace HMS.Ordering.API.Application.IntegrationEvents.EventHandling
{
    public class OrderPaymentSuccededIntegrationEventHandler : 
        IIntegrationEventHandler<OrderPaymentSuccededIntegrationEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public OrderPaymentSuccededIntegrationEventHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Handle(OrderPaymentSuccededIntegrationEvent @event)
        {
			Order orderToUpdate = await _orderRepository.GetAsync(@event.OrderId);

            orderToUpdate.SetPaidStatus();

            await _orderRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }
}