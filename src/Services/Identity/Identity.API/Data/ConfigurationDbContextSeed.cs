using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using HMS.Identity.API.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Identity.API.Data
{
    public class ConfigurationDbContextSeed
    {
		private readonly ConfigurationDbContext _context;
		private readonly IConfiguration _configuration;

		public ConfigurationDbContextSeed(ConfigurationDbContext context, IConfiguration configuration)
		{
			this._context = context;
			this._configuration = configuration;
		}
		public async Task SeedAsync()
        {
            //callbacks urls from config:
            var clientUrls = new Dictionary<string, string>
            {
                {"Mvc", _configuration.GetValue<string>("MvcClient")},
                {"Spa", _configuration.GetValue<string>("SpaClient")},
                {"Xamarin", _configuration.GetValue<string>("XamarinCallback")},
                {"LocationsApi", _configuration.GetValue<string>("LocationApiClient")},
                {"MarketingApi", _configuration.GetValue<string>("MarketingApiClient")},
                {"BasketApi", _configuration.GetValue<string>("BasketApiClient")},
				{"CatalogApi", _configuration.GetValue<string>("CatalogApiClient")},
				{"OrderingApi", _configuration.GetValue<string>("OrderingApiClient")}
            };

            if (!await _context.Clients.AnyAsync())
            {
                await _context.Clients.AddRangeAsync(Config.GetClients(clientUrls).Select(client => client.ToEntity()));
            }

            if (!await _context.IdentityResources.AnyAsync())
            {
                await _context.IdentityResources.AddRangeAsync(Config.GetResources().Select(resource => resource.ToEntity()));
            }

            if (!await _context.ApiResources.AnyAsync())
            {
                await _context.ApiResources.AddRangeAsync(Config.GetApis().Select(api => api.ToEntity()));
            }

            await _context.SaveChangesAsync();
        }
    }
}
