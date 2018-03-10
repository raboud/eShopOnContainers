using System;
using System.Threading.Tasks;
using HMS.Core.Services.RequestProvider;
using HMS.Core.Models.Basket;
using HMS.Core.Services.FixUri;
using HMS.IntegrationEvents;

namespace HMS.Core.Services.Basket
{
    public class BasketService : IBasketService
    {
        private readonly IRequestProvider _requestProvider;
        private readonly IFixUriService _fixUriService;

        private const string ApiUrlBase = "api/v1/basket";

        public BasketService(IRequestProvider requestProvider, IFixUriService fixUriService)
        {
            _requestProvider = requestProvider;
            _fixUriService = fixUriService;
        }

        public async Task<CustomerBasket> GetBasketAsync(string guidUser, string token)
        {
			UriBuilder builder = new UriBuilder(GlobalSetting.Instance.BasketEndpoint)
            {
                Path = $"{ApiUrlBase}/{guidUser}"
            };

			string uri = builder.ToString();

            CustomerBasket basket =
                    await _requestProvider.GetAsync<CustomerBasket>(uri, token);

            _fixUriService.FixBasketItemPictureUri(basket?.Items);
            return basket;
        }

        public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket customerBasket, string token)
        {
			UriBuilder builder = new UriBuilder(GlobalSetting.Instance.BasketEndpoint)
            {
                Path = ApiUrlBase
            };

			string uri = builder.ToString();
			CustomerBasket result = await _requestProvider.PostAsync(uri, customerBasket, token);
            return result;
        }

        public async Task CheckoutAsync(BasketCheckout basketCheckout, string token)
        {
			UriBuilder builder = new UriBuilder(GlobalSetting.Instance.BasketEndpoint)
            {
                Path = $"{ApiUrlBase}/checkout"
            };

			string uri = builder.ToString();
            await _requestProvider.PostAsync(uri, basketCheckout, token);
        }

        public async Task ClearBasketAsync(string guidUser, string token)
        {
			UriBuilder builder = new UriBuilder(GlobalSetting.Instance.BasketEndpoint)
            {
                Path = $"{ApiUrlBase}/{guidUser}"
            };

			string uri = builder.ToString();
            await _requestProvider.DeleteAsync(uri, token);
        }
    }
}