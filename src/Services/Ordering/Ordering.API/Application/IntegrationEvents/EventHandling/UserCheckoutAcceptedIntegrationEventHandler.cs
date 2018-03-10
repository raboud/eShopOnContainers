using HMS.IntegrationEvents.Events;
using HMS.Ordering.API.Application.Commands;
using MediatR;
using Microsoft.BuildingBlocks.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HMS.Ordering.API.Application.IntegrationEvents.EventHandling
{
	public class UserCheckoutAcceptedIntegrationEventHandler : IIntegrationEventHandler<UserCheckoutAcceptedIntegrationEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILoggerFactory _logger;
        private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

        public UserCheckoutAcceptedIntegrationEventHandler(IMediator mediator,
            ILoggerFactory logger, IOrderingIntegrationEventService orderingIntegrationEventService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentNullException(nameof(orderingIntegrationEventService));
        }

        /// <summary>
        /// Integration event handler which starts the create order process
        /// </summary>
        /// <param name="eventMsg">
        /// Integration event message which is sent by the
        /// basket.api once it has successfully process the 
        /// order items.
        /// </param>
        /// <returns></returns>
        public async Task Handle(UserCheckoutAcceptedIntegrationEvent eventMsg)
        {
			bool result = false;

			// Send Integration event to clean basket once basket is converted to Order and before starting with the order creation process
			OrderStartedIntegrationEvent orderStartedIntegrationEvent = new OrderStartedIntegrationEvent(eventMsg.UserId);
            await _orderingIntegrationEventService.PublishThroughEventBusAsync(orderStartedIntegrationEvent);

            if (eventMsg.RequestId != Guid.Empty)
            {
				CreateOrderCommand createOrderCommand = new CreateOrderCommand(eventMsg.Basket.Items, eventMsg.UserId, eventMsg.City, eventMsg.Street, 
                    eventMsg.State, eventMsg.Country, eventMsg.ZipCode,
                    eventMsg.CardNumber, eventMsg.CardHolderName, eventMsg.CardExpiration,
                    eventMsg.CardSecurityNumber, eventMsg.CardTypeId);

				IdentifiedCommand<CreateOrderCommand, bool> requestCreateOrder = new IdentifiedCommand<CreateOrderCommand, bool>(createOrderCommand, eventMsg.RequestId);
                result = await _mediator.Send(requestCreateOrder);
            }            

            _logger.CreateLogger(nameof(UserCheckoutAcceptedIntegrationEventHandler))
                .LogTrace(result ? $"UserCheckoutAccepted integration event has been received and a create new order process is started with requestId: {eventMsg.RequestId}" : 
                    $"UserCheckoutAccepted integration event has been received but a new order process has failed with requestId: {eventMsg.RequestId}");
        }
    }
}