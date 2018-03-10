using Microsoft.BuildingBlocks.EventBus.Events;

namespace HMS.IntegrationEvents.Events
{
    public class OrderStockConfirmedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; }

        public OrderStockConfirmedIntegrationEvent(int orderId) => OrderId = orderId;
    }
}