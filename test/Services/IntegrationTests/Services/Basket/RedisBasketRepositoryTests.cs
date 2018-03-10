namespace HMS.IntegrationTests.Services.Basket
{
    using HMS.Basket.API;
    using HMS.Basket.API.Model;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;
    using Moq;
    using StackExchange.Redis;
	using HMS.IntegrationEvents;

	public class RedisBasketRepositoryTests
    {
        private Mock<IOptionsSnapshot<BasketSettings>> _optionsMock;

        public RedisBasketRepositoryTests()
        {
            _optionsMock = new Mock<IOptionsSnapshot<BasketSettings>>();
        }

        [Fact]
        public async Task UpdateBasket_return_and_add_basket()
        {
			RedisBasketRepository redisBasketRepository = BuildBasketRepository();

			CustomerBasket basket = await redisBasketRepository.UpdateBasketAsync(new CustomerBasket("customerId")
            {
                BuyerId = "buyerId",
                Items = BuildBasketItems()
            });

            Assert.NotNull(basket);
            Assert.Single(basket.Items);
        }

        [Fact]
        public async Task Delete_Basket_return_null()
        {
			RedisBasketRepository redisBasketRepository = BuildBasketRepository();

			CustomerBasket basket = await redisBasketRepository.UpdateBasketAsync(new CustomerBasket("customerId")
            {
                BuyerId = "buyerId",
                Items = BuildBasketItems()
            });

			bool deleteResult = await redisBasketRepository.DeleteBasketAsync("buyerId");

			CustomerBasket result = await redisBasketRepository.GetBasketAsync(basket.BuyerId);

            Assert.True(deleteResult);
            Assert.Null(result);
        }

        RedisBasketRepository BuildBasketRepository()
        {
			LoggerFactory loggerFactory = new LoggerFactory();
			ConfigurationOptions configuration = ConfigurationOptions.Parse("127.0.0.1", true);
            configuration.ResolveDns = true;
            return new RedisBasketRepository(loggerFactory, ConnectionMultiplexer.Connect(configuration));
        }

        List<BasketItem> BuildBasketItems()
        {
            return new List<BasketItem>()
            {
                new BasketItem()
                {
                    Id = "basketId",
                    PictureUrl = "pictureurl",
                    ProductId = "productId",
                    ProductName = "productName",
                    Quantity = 1,
                    UnitPrice = 1
                }
            };
        }
    }
}
