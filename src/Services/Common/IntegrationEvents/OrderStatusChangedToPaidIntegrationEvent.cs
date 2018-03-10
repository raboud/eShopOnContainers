using System.Collections.Generic;
using Microsoft.BuildingBlocks.EventBus.Events;

namespace HMS.IntegrationEvents.Events
{

    public class OrderStatusChangedToPaidIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; }
        public IEnumerable<OrderStockItem> OrderStockItems { get; }

        public OrderStatusChangedToPaidIntegrationEvent(int orderId,
            IEnumerable<OrderStockItem> orderStockItems)
        {
            OrderId = orderId;
            OrderStockItems = orderStockItems;
        }
    }
}