using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.eShopOnContainers.Services.Identity.API;
using Microsoft.eShopOnContainers.Services.Identity.API.Data;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionalTests.Services.Identity
{
	class IdentityScenariosBase
    {
		public IdentityServer CreateServer()
		{
			var webHostBuilder = WebHost.CreateDefaultBuilder();
			webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory() + "\\Services\\Identity");
			webHostBuilder.UseStartup<Startup>();

			var testServer = new IdentityServer(webHostBuilder);

			testServer.Host
			   .MigrateDbContext<PersistedGrantDbContext>((_, __) => { })
				.MigrateDbContext<ApplicationDbContext>((context, services) =>
				{
					services.GetService<ApplicationDbContextSeed>()
						.SeedAsync()
						.Wait();
				})
				.MigrateDbContext<ConfigurationDbContext>((context, services) =>
				{
					services.GetService<ConfigurationDbContextSeed>()
						.SeedAsync()
						.Wait();
				});		
			return testServer;
		}
	}

	public class IdentityServer : TestServer
	{
		public IdentityServer(IWebHostBuilder builder)
			: base(builder) { }

		public IdentityServer(IWebHostBuilder builder, IFeatureCollection featureCollection)
			: base(builder, featureCollection) { }

		public async Task<string> GetTokenAsync(string username, string password, string clientId, string clientSecret)
		{
			HttpClient idClient = this.CreateClient();
			var formContent = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("grant_type", "password"),
				new KeyValuePair<string, string>("username", username),
				new KeyValuePair<string, string>("password", password),
				new KeyValuePair<string, string>("client_id", clientId),
				new KeyValuePair<string, string>("client_secret", clientSecret)
			});

			var response = await idClient.PostAsync("connect/token", formContent);
			var stringContent = await response.Content.ReadAsStringAsync();
			var temp = JObject.Parse(stringContent);
			return (string)temp.Property("access_token");
		}


	}
}
