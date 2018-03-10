using MediatR;
using HMS.Ordering.Domain.AggregatesModel.OrderAggregate;
using Microsoft.Extensions.Logging;
using HMS.Ordering.Domain.Events;
using System;
using System.Threading.Tasks;
using HMS.Ordering.API.Application.IntegrationEvents;
using System.Linq;
using System.Threading;
using HMS.IntegrationEvents.Events;
using System.Collections.Generic;

namespace HMS.Ordering.API.Application.DomainEventHandlers.OrderPaid
{

    public class OrderStatusChangedToPaidDomainEventHandler
                   : INotificationHandler<OrderStatusChangedToPaidDomainEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILoggerFactory _logger;
        private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

        public OrderStatusChangedToPaidDomainEventHandler(
            IOrderRepository orderRepository, ILoggerFactory logger,
            IOrderingIntegrationEventService orderingIntegrationEventService)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderingIntegrationEventService = orderingIntegrationEventService;
        }

        public async Task Handle(OrderStatusChangedToPaidDomainEvent orderStatusChangedToPaidDomainEvent, CancellationToken cancellationToken)
        {
            _logger.CreateLogger(nameof(OrderStatusChangedToPaidDomainEventHandler))
             .LogTrace($"Order with Id: {orderStatusChangedToPaidDomainEvent.OrderId} has been successfully updated with " +
                       $"a status order id: {OrderStatus.Paid.Id}");

			IEnumerable<OrderStockItem> orderStockList = orderStatusChangedToPaidDomainEvent.OrderItems
                .Select(orderItem => new OrderStockItem(orderItem.ProductId, orderItem.GetUnits()));

			OrderStatusChangedToPaidIntegrationEvent orderStatusChangedToPaidIntegrationEvent = new OrderStatusChangedToPaidIntegrationEvent(orderStatusChangedToPaidDomainEvent.OrderId,
                orderStockList);
            await _orderingIntegrationEventService.PublishThroughEventBusAsync(orderStatusChangedToPaidIntegrationEvent);
        }
    }  
}