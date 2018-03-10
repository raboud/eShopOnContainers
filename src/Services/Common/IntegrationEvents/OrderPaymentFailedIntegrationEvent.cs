using Microsoft.BuildingBlocks.EventBus.Events;

namespace HMS.IntegrationEvents.Events
{

    public class OrderPaymentFailedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; }

        public OrderPaymentFailedIntegrationEvent(int orderId) => OrderId = orderId;
    }
}