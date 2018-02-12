using HMS.FunctionalTests.Services.Identity;
using HMS.FunctionalTests.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HMS.FunctionalTests.Services.Catalog
{
    public class CatalogScenarios 
    {
		[Fact]
		public async Task SettingPriceAdmin()
		{
			using (var idServer = new IdentityScenariosBase().CreateServer())
			using (var catalogServer = new CatalogScenariosBase(idServer))
			{
				var accessToken = await idServer.GetTokenAsync("demoadmin@microsoft.com", "Pass@word1", "ro.client", "secret");

				var catalogClient = catalogServer.CreateClient();
				catalogClient.SetBearerToken(accessToken);

				var originalCatalogProducts = await catalogClient.GetCatalogAsync();

				var product = originalCatalogProducts.Data.First();
				product.Price += 2;
				var resp = await catalogClient.UpdateProduct(product);
				Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
				var p2 = await catalogClient.GetCatalogItemAsync(product.Id);

				Assert.Equal(product.Price, p2.Price);

				product.Price -= 2;
				resp = await catalogClient.UpdateProduct(product);
				Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);


			}
		}

		[Fact]
		public async Task SettingPriceUser()
		{
			using (var idServer = new IdentityScenariosBase().CreateServer())
			using (var catalogServer = new CatalogScenariosBase(idServer))
			{
				var accessToken = await idServer.GetTokenAsync("demouser@microsoft.com", "Pass@word1", "ro.client", "secret");

				var catalogClient = catalogServer.CreateClient();
				catalogClient.SetBearerToken(accessToken);

				var originalCatalogProducts = await catalogClient.GetCatalogAsync();

				var product = originalCatalogProducts.Data.First();
				product.Price += 2;
				var resp = await catalogClient.UpdateProduct(product);
				Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
			}
		}

		[Fact]
		public async Task SettingPriceAnonymous()
		{
			using (var idServer = new IdentityScenariosBase().CreateServer())
			using (var catalogServer = new CatalogScenariosBase(idServer))
			{
				var catalogClient = catalogServer.CreateClient();

				var originalCatalogProducts = await catalogClient.GetCatalogAsync();

				var product = originalCatalogProducts.Data.First();
				product.Price += 2;
				var resp = await catalogClient.UpdateProduct(product);
				Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
			}
		}

	}
}
