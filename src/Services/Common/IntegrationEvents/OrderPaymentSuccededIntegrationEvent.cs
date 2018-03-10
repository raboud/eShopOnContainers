using Microsoft.BuildingBlocks.EventBus.Events;

namespace HMS.IntegrationEvents.Events
{
    public class OrderPaymentSuccededIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; }

        public OrderPaymentSuccededIntegrationEvent(int orderId) => OrderId = orderId;
    }
}