using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;

namespace HMS.Catalog.API.IntegrationEvents.Events
{
    public class OrderStockConfirmedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; }

        public OrderStockConfirmedIntegrationEvent(int orderId) => OrderId = orderId;
    }
}