using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.BuildingBlocks.Resilience.Http;
using HMS.WebMVC.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using HMS.WebMVC.Infrastructure;
using HMS.WebMVC.Models;
using HMS.IntegrationEvents;
using System.Net.Http;

namespace HMS.WebMVC.Services
{
    public class BasketService : IBasketService
    {
        private readonly IOptionsSnapshot<AppSettings> _settings;
        private IHttpClient _apiClient;
        private readonly string _remoteServiceBaseUrl;
        private IHttpContextAccessor _httpContextAccesor;

        public BasketService(IOptionsSnapshot<AppSettings> settings, IHttpContextAccessor httpContextAccesor, IHttpClient httpClient)
        {
            _settings = settings;
            _remoteServiceBaseUrl = $"{_settings.Value.BasketUrl}/api/v1/basket";
            _httpContextAccesor = httpContextAccesor;
            _apiClient = httpClient;
        }

        public async Task<Basket> GetBasket(ApplicationUser user)
        {
			string token = await GetUserTokenAsync();
			string getBasketUri = API.Basket.GetBasket(_remoteServiceBaseUrl, user.Id);

			string dataString = await _apiClient.GetStringAsync(getBasketUri, token);

			// Use the ?? Null conditional operator to simplify the initialization of response
			Basket response = JsonConvert.DeserializeObject<Basket>(dataString) ??
                new Basket()
                {
                    BuyerId = user.Id
                };

            return response;
        }

        public async Task<Basket> UpdateBasket(Basket basket)
        {
			string token = await GetUserTokenAsync();
			string updateBasketUri = API.Basket.UpdateBasket(_remoteServiceBaseUrl);

			HttpResponseMessage response = await _apiClient.PostAsync(updateBasketUri, basket, token);

            response.EnsureSuccessStatusCode();

            return basket;
        }

        public async Task Checkout(BasketDTO basket)
        {
			string token = await GetUserTokenAsync();
			string updateBasketUri = API.Basket.CheckoutBasket(_remoteServiceBaseUrl);

			HttpResponseMessage response = await _apiClient.PostAsync(updateBasketUri, basket, token);

            response.EnsureSuccessStatusCode();
        }

        public async Task<Basket> SetQuantities(ApplicationUser user, Dictionary<string, int> quantities)
        {
			Basket basket = await GetBasket(user);

            basket.Items.ForEach(x =>
            {
                // Simplify this logic by using the
                // new out variable initializer.
                if (quantities.TryGetValue(x.Id, out int quantity))
                {
                    x.Quantity = quantity;
                }
            });

            return basket;
        }

        public Order MapBasketToOrder(Basket basket)
        {
			Order order = new Order()
			{
				Total = 0
			};

            basket.Items.ForEach(x =>
            {
                order.OrderItems.Add(new OrderItem()
                {
                    ProductId = int.Parse(x.ProductId),

                    PictureUrl = x.PictureUrl,
                    ProductName = x.ProductName,
                    Units = x.Quantity,
                    UnitPrice = x.UnitPrice
                });
                order.Total += (x.Quantity * x.UnitPrice);
            });

            return order;
        }

        public async Task AddItemToBasket(ApplicationUser user, BasketItem product)
        {
			Basket basket = await GetBasket(user);

            if (basket == null)
            {
                basket = new Basket()
                {
                    BuyerId = user.Id,
                    Items = new List<BasketItem>()
                };
            }

            basket.Items.Add(product);

            await UpdateBasket(basket);
        }        

        async Task<string> GetUserTokenAsync()
        {
			HttpContext context = _httpContextAccesor.HttpContext;
            return await context.GetTokenAsync("access_token");
        }
    }
}
