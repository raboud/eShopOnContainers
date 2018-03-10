using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.BuildingBlocks.EventBus.Abstractions;
using HMS.Catalog.API.Infrastructure;
using Microsoft.BuildingBlocks.EventBus.Events;
using HMS.IntegrationEvents.Events;

namespace HMS.Catalog.API.IntegrationEvents.EventHandling
{
    public class OrderStatusChangedToAwaitingValidationIntegrationEventHandler : 
        IIntegrationEventHandler<OrderStatusChangedToAwaitingValidationIntegrationEvent>
    {
        private readonly CatalogContext _catalogContext;
        private readonly ICatalogIntegrationEventService _catalogIntegrationEventService;

        public OrderStatusChangedToAwaitingValidationIntegrationEventHandler(CatalogContext catalogContext,
            ICatalogIntegrationEventService catalogIntegrationEventService)
        {
            _catalogContext = catalogContext;
            _catalogIntegrationEventService = catalogIntegrationEventService;
        }

        public async Task Handle(OrderStatusChangedToAwaitingValidationIntegrationEvent command)
		{
			List<ConfirmedOrderStockItem> confirmedOrderStockItems = new List<ConfirmedOrderStockItem>();

			foreach (OrderStockItem orderStockItem in command.OrderStockItems)
			{
				Model.Product catalogItem = _catalogContext.Products.Find(orderStockItem.ProductId);
				bool hasStock = catalogItem.AvailableStock >= orderStockItem.Units;
				ConfirmedOrderStockItem confirmedOrderStockItem = new ConfirmedOrderStockItem(catalogItem.Id, hasStock);

				confirmedOrderStockItems.Add(confirmedOrderStockItem);
			}

			IntegrationEvent confirmedIntegrationEvent = confirmedOrderStockItems.Any(c => !c.HasStock)
				? (IntegrationEvent)new OrderStockRejectedIntegrationEvent(command.OrderId, confirmedOrderStockItems)
				: new OrderStockConfirmedIntegrationEvent(command.OrderId);

			await NewMethod(confirmedIntegrationEvent);
			await _catalogIntegrationEventService.PublishThroughEventBusAsync(confirmedIntegrationEvent);
		}

		private async Task NewMethod(IntegrationEvent confirmedIntegrationEvent)
		{
			await _catalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync(confirmedIntegrationEvent);
		}
	}
}