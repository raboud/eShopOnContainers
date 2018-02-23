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
                _context.Clients.AddRange(Config.GetClients(clientUrls).Select(client => client.ToEntity()));
            }

            if (!await _context.IdentityResources.AnyAsync())
            {
                _context.IdentityResources.AddRange(Config.GetResources().Select(resource => resource.ToEntity()));
            }

            if (!await _context.ApiResources.AnyAsync())
            {
                _context.ApiResources.AddRange(Config.GetApis().Select(api => api.ToEntity()));
            }

            await _context.SaveChangesAsync();
        }
    }
}
