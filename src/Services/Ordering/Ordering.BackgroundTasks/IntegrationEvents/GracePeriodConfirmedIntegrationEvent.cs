using Microsoft.BuildingBlocks.EventBus.Events;

namespace HMS.Ordering.BackgroundTasks.IntegrationEvents
{
    public class GracePeriodConfirmedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; }

        public GracePeriodConfirmedIntegrationEvent(int orderId) =>
            OrderId = orderId;
    }
}
