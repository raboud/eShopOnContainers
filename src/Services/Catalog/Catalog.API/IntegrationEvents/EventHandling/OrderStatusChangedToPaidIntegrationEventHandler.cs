using System.Threading.Tasks;
using Microsoft.BuildingBlocks.EventBus.Abstractions;
using HMS.Catalog.API.Infrastructure;
using HMS.IntegrationEvents.Events;

namespace HMS.Catalog.API.IntegrationEvents.EventHandling
{
    public class OrderStatusChangedToPaidIntegrationEventHandler : 
        IIntegrationEventHandler<OrderStatusChangedToPaidIntegrationEvent>
    {
        private readonly CatalogContext _catalogContext;

        public OrderStatusChangedToPaidIntegrationEventHandler(CatalogContext catalogContext)
        {
            _catalogContext = catalogContext;
        }

        public async Task Handle(OrderStatusChangedToPaidIntegrationEvent command)
        {
            //we're not blocking stock/inventory
            foreach (OrderStockItem orderStockItem in command.OrderStockItems)
            {
				Model.Product catalogItem = _catalogContext.Products.Find(orderStockItem.ProductId);

                catalogItem.RemoveStock(orderStockItem.Units);
            }

            await _catalogContext.SaveChangesAsync();
        }
    }
}