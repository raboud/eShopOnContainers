using HMS.IntegrationEvents.Events;
using HMS.Ordering.API.Application.IntegrationEvents;
using HMS.Ordering.Domain.AggregatesModel.OrderAggregate;
using HMS.Ordering.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HMS.Ordering.API.Application.DomainEventHandlers.OrderGracePeriodConfirmed
{
	public class OrderStatusChangedToAwaitingValidationDomainEventHandler
                   : INotificationHandler<OrderStatusChangedToAwaitingValidationDomainEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILoggerFactory _logger;
        private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

        public OrderStatusChangedToAwaitingValidationDomainEventHandler(
            IOrderRepository orderRepository, ILoggerFactory logger,
            IOrderingIntegrationEventService orderingIntegrationEventService)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderingIntegrationEventService = orderingIntegrationEventService;
        }

        public async Task Handle(OrderStatusChangedToAwaitingValidationDomainEvent orderStatusChangedToAwaitingValidationDomainEvent, CancellationToken cancellationToken)
        {
            _logger.CreateLogger(nameof(OrderStatusChangedToAwaitingValidationDomainEvent))
                  .LogTrace($"Order with Id: {orderStatusChangedToAwaitingValidationDomainEvent.OrderId} has been successfully updated with " +
                            $"a status order id: {OrderStatus.AwaitingValidation.Id}");

			IEnumerable<OrderStockItem> orderStockList = orderStatusChangedToAwaitingValidationDomainEvent.OrderItems
                .Select(orderItem => new OrderStockItem(orderItem.ProductId, orderItem.GetUnits()));

			OrderStatusChangedToAwaitingValidationIntegrationEvent orderStatusChangedToAwaitingValidationIntegrationEvent = new OrderStatusChangedToAwaitingValidationIntegrationEvent(
                orderStatusChangedToAwaitingValidationDomainEvent.OrderId, orderStockList);
            await _orderingIntegrationEventService.PublishThroughEventBusAsync(orderStatusChangedToAwaitingValidationIntegrationEvent);
        }
    }  
}