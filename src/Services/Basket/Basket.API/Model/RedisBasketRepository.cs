using HMS.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Basket.API.Model
{
    public class RedisBasketRepository : IBasketRepository
    {
        private readonly ILogger<RedisBasketRepository> _logger;

        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisBasketRepository(ILoggerFactory loggerFactory, ConnectionMultiplexer redis)
        {
            _logger = loggerFactory.CreateLogger<RedisBasketRepository>();
            _redis = redis;
            _database = redis.GetDatabase();
        }

        public async Task<bool> DeleteBasketAsync(string id)
        {
            return await _database.KeyDeleteAsync(id);
        }

        public IEnumerable<string> GetUsers()
        {
			IServer server = GetServer();
			IEnumerable<RedisKey> data = server.Keys();
            return data?.Select(k => k.ToString());
        }

        public async Task<CustomerBasket> GetBasketAsync(string customerId)
        {
			RedisValue data = await _database.StringGetAsync(customerId);
            if (data.IsNullOrEmpty)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<CustomerBasket>(data);
        }

        public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
        {
			bool created = await _database.StringSetAsync(basket.BuyerId, JsonConvert.SerializeObject(basket));
            if (!created)
            {
                _logger.LogInformation("Problem occur persisting the item.");
                return null;
            }

            _logger.LogInformation("Basket item persisted succesfully.");

            return await GetBasketAsync(basket.BuyerId);
        }

        private IServer GetServer()
        {
			System.Net.EndPoint[] endpoint = _redis.GetEndPoints();
            return _redis.GetServer(endpoint.First());
        }
    }
}
