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
			using (IdentityServer idServer = new IdentityScenariosBase().CreateServer())
			using (CatalogScenariosBase catalogServer = new CatalogScenariosBase(idServer))
			{
				string accessToken = await idServer.GetTokenAsync("demoadmin@microsoft.com", "Pass@word1", "ro.client", "secret");

				CatalogClient catalogClient = catalogServer.CreateClient();
				catalogClient.SetBearerToken(accessToken);

				Common.API.PaginatedItemsViewModel<HMS.Catalog.DTO.ProductDTO> originalCatalogProducts = await catalogClient.GetCatalogAsync();

				HMS.Catalog.DTO.ProductDTO product = originalCatalogProducts.Data.First();
				product.Price += 2;
				HttpResponseMessage resp = await catalogClient.UpdateProduct(product);
				Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
				HMS.Catalog.DTO.ProductDTO p2 = await catalogClient.GetCatalogItemAsync(product.Id);

				Assert.Equal(product.Price, p2.Price);

				product.Price -= 2;
				resp = await catalogClient.UpdateProduct(product);
				Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);


			}
		}

		[Fact]
		public async Task SettingPriceUser()
		{
			using (IdentityServer idServer = new IdentityScenariosBase().CreateServer())
			using (CatalogScenariosBase catalogServer = new CatalogScenariosBase(idServer))
			{
				string accessToken = await idServer.GetTokenAsync("demouser@microsoft.com", "Pass@word1", "ro.client", "secret");

				CatalogClient catalogClient = catalogServer.CreateClient();
				catalogClient.SetBearerToken(accessToken);

				Common.API.PaginatedItemsViewModel<HMS.Catalog.DTO.ProductDTO> originalCatalogProducts = await catalogClient.GetCatalogAsync();

				HMS.Catalog.DTO.ProductDTO product = originalCatalogProducts.Data.First();
				product.Price += 2;
				HttpResponseMessage resp = await catalogClient.UpdateProduct(product);
				Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
			}
		}

		[Fact]
		public async Task SettingPriceAnonymous()
		{
			using (IdentityServer idServer = new IdentityScenariosBase().CreateServer())
			using (CatalogScenariosBase catalogServer = new CatalogScenariosBase(idServer))
			{
				CatalogClient catalogClient = catalogServer.CreateClient();

				Common.API.PaginatedItemsViewModel<HMS.Catalog.DTO.ProductDTO> originalCatalogProducts = await catalogClient.GetCatalogAsync();

				HMS.Catalog.DTO.ProductDTO product = originalCatalogProducts.Data.First();
				product.Price += 2;
				HttpResponseMessage resp = await catalogClient.UpdateProduct(product);
				Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
			}
		}

	}
}
