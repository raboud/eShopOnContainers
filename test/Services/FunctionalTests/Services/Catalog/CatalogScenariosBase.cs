using FunctionalTests.Services.Identity;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.eShopOnContainers.BuildingBlocks.IntegrationEventLogEF;
using Microsoft.eShopOnContainers.Services.Catalog.API;
using Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure;
using Microsoft.eShopOnContainers.Services.Catalog.API.Model;
using Microsoft.eShopOnContainers.Services.Catalog.API.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalTests.Services.Catalog
{
    public class CatalogScenariosBase : IDisposable
    {
		private TestServer testServer { get; set; }

		public CatalogScenariosBase(IdentityServer idServer)
		{
			CreateServer(idServer);
		}

		public CatalogClient CreateClient()
		{
			return new CatalogClient(this.testServer.CreateHandler()) { BaseAddress = this.testServer.BaseAddress };
		}

		private void CreateServer(IdentityServer idServer)
        {
			TestStartup.BackChannelHandler = idServer.CreateHandler();
			var webHostBuilder = WebHost.CreateDefaultBuilder();
            webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory() + "\\Services\\Catalog");
            webHostBuilder.UseStartup<TestStartup>();

            this.testServer = new TestServer(webHostBuilder);

            this.testServer.Host
                .MigrateDbContext<CatalogContext>((context, services) =>
                {
                    var env = services.GetService<IHostingEnvironment>();
                    var settings = services.GetService<IOptions<CatalogSettings>>();
                    var logger = services.GetService<ILogger<CatalogContextSeed>>();

                    new CatalogContextSeed()
                    .SeedAsync(context, env, settings, logger)
                    .Wait();
                })
                .MigrateDbContext<IntegrationEventLogContext>((_, __) => { });
        }

		public void Dispose()
		{
			this.testServer.Dispose();
		}

		public static class Get
        {
            public static string Orders = "api/v1/orders";

            public static string Items = "api/v1/catalog/items";

            public static string ProductByName(string name)
            {
                return $"api/v1/catalog/items/withname/{name}";
            }
        }

        public static class Put
        {
            public static string UpdateCatalogProduct = "api/v1/catalog/items";
        }

		public class TestStartup : Startup
		{
			public TestStartup(IConfiguration configuration) : base(configuration) { }

			public static HttpMessageHandler BackChannelHandler { get; set; }

			public override void setIS4Options(IdentityServerAuthenticationOptions options)
			{
				base.setIS4Options(options);
				options.IntrospectionBackChannelHandler = BackChannelHandler;
				options.IntrospectionDiscoveryHandler = BackChannelHandler;
				options.JwtBackChannelHandler = BackChannelHandler;
			}

		}
	}

	public class CatalogClient : HttpClient
	{
		public CatalogClient(HttpMessageHandler handler) : base(handler) { }

		public async Task<PaginatedItemsViewModel<CatalogItem>> GetCatalogAsync()
		{
			var response = await this.GetAsync(CatalogScenariosBase.Get.Items);
			var items = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<PaginatedItemsViewModel<CatalogItem>>(items);
		}

		public async Task<HttpResponseMessage> UpdateProduct(CatalogItem product)
		{
			var content = new StringContent(JsonConvert.SerializeObject(product), UTF8Encoding.UTF8, "application/json");
			return await this.PutAsync(CatalogScenariosBase.Put.UpdateCatalogProduct, content);
		}

		public async Task<CatalogItem> GetCatalogItemAsync(int id)
		{
			var response = await this.GetAsync(CatalogScenariosBase.Get.Items + $"/{id}");
			var items = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<CatalogItem>(items);
		}

	}
}
