using Microsoft.BuildingBlocks.EventBus.Abstractions;
using HMS.Basket.API.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using HMS.IntegrationEvents;
using HMS.IntegrationEvents.Events;

namespace HMS.Basket.API.IntegrationEvents.EventHandling
{
	public class ProductPriceChangedIntegrationEventHandler : IIntegrationEventHandler<ProductPriceChangedIntegrationEvent>
	{
		private readonly IBasketRepository _repository;

		public ProductPriceChangedIntegrationEventHandler(IBasketRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		public async Task Handle(ProductPriceChangedIntegrationEvent @event)
		{
			System.Collections.Generic.IEnumerable<string> userIds = _repository.GetUsers();

			foreach (string id in userIds)
			{
				CustomerBasket basket = await _repository.GetBasketAsync(id);

				await UpdatePriceInBasketItems(@event.ProductId, @event.NewPrice, @event.OldPrice, basket);
			}
		}

		private async Task UpdatePriceInBasketItems(int productId, decimal newPrice, decimal oldPrice, CustomerBasket basket)
		{
			string match = productId.ToString();
			List<BasketItem> itemsToUpdate = basket?.Items?.Where(x => x.ProductId == match).ToList();

			if (itemsToUpdate != null)
			{
				foreach (BasketItem item in itemsToUpdate)
				{
					if (item.UnitPrice == oldPrice)
					{
						decimal originalPrice = item.UnitPrice;
						item.UnitPrice = newPrice;
						item.OldUnitPrice = originalPrice;
					}
				}
				await _repository.UpdateBasketAsync(basket);
			}
		}
	}
}

