using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using HMS.Catalog.API.Infrastructure.EntityConfigurations;
using HMS.Catalog.API.Model;
using Microsoft.EntityFrameworkCore;

namespace HMS.Catalog.API.Infrastructure
{

	public class CatalogContextSeed
	{
		public async Task SeedAsync(CatalogContext context, IHostingEnvironment env, IOptions<CatalogSettings> settings, ILogger<CatalogContextSeed> logger)
		{
			var policy = CreatePolicy(logger, nameof(CatalogContextSeed));

			await policy.ExecuteAsync(async () =>
			{
				var useCustomizationData = settings.Value.UseCustomizationData;
				var contentRootPath = env.ContentRootPath;
				var picturePath = env.WebRootPath;

				string fileName = Path.Combine(contentRootPath, "Setup", "Catalog.json");
				if (File.Exists(fileName))
				{
					string raw = File.ReadAllText(fileName);
					dynamic data = JObject.Parse(raw);

					await processBrands(data.Brands, context, logger);
					await processVendors(data.Vendors, context, logger);
					await processUnits(data.Units, context, logger);
					await processCategories(data.Categories, context, logger);
					await processProducts(data.Products, context, logger);
				}

				GetCatalogItemPictures(contentRootPath, picturePath);
			});
		}

		async static Task processBrands(dynamic items, CatalogContext context, ILogger<CatalogContextSeed> logger)
		{
			List<Brand> data = await context.Brands.ToListAsync();
			foreach (string item in items)
			{
				if (!data.Any(b => b.Name == item))
				{
					context.Brands.Add(new Brand() { Name = item });
				}
			}
			await context.SaveChangesAsync();
		}

		async static Task processVendors(dynamic items, CatalogContext context, ILogger<CatalogContextSeed> logger)
		{
			List<Vendor> data = await context.Vendors.ToListAsync();
			foreach (string item in items)
			{
				if (!data.Any(b => b.Name == item))
				{
					context.Vendors.Add(new Vendor() { Name = item });
				}
			}
			await context.SaveChangesAsync();
		}
		async static Task processUnits(dynamic items, CatalogContext context, ILogger<CatalogContextSeed> logger)
		{
			List<Unit> data = await context.Units.ToListAsync();
			foreach (string item in items)
			{
				if (!data.Any(b => b.Name == item))
				{
					context.Units.Add(new Unit() { Name = item });
				}
			}
			await context.SaveChangesAsync();
		}

		async static Task processCategories(dynamic items, CatalogContext context, ILogger<CatalogContextSeed> logger)
		{
			List<Category> data = await context.Categories.ToListAsync();
			foreach (string item in items)
			{
				if (!data.Any(b => b.Name == item))
				{
					context.Categories.Add(new Category() { Name = item });
				}
			}
			await context.SaveChangesAsync();
		}

		async static Task processProducts(dynamic items, CatalogContext context, ILogger<CatalogContextSeed> logger)
		{
			List<Brand> brands = await context.Brands.ToListAsync();
			List<Vendor> vendors = await context.Vendors.ToListAsync();
			List<Category> categories = await context.Categories.ToListAsync();
			List<Unit> units = await context.Units.ToListAsync();


			List<ProductCategory> pcs = new List<ProductCategory>();

			List<Product> data = await context.Products.ToListAsync();
			foreach (dynamic item in items)
			{
				if (!data.Any(b => b.Name == (string) item.Name && b.Count == (int) item.Count))
				{
					Brand brand = brands.FirstOrDefault(b => b.Name == (string) item.Brand);
					Unit unit = units.FirstOrDefault(b => b.Name == (string) item.Unit);
					Vendor vendor = vendors.FirstOrDefault(b => b.Name == (string) item.Vendor);

					if (brand == null || unit == null || vendor == null)
					{
						string name = item.Name;
						logger.LogError("invalid item", name);
						return;
					}
					Product p = new Product();
					p.Brand = brand;
					p.Unit = unit;
					p.Vendor = vendor;
					p.BrandId = p.Brand.Id;
					p.UnitId = p.Unit.Id;
					p.VendorId = p.Vendor.Id;

					p.Id = item.Id;
					p.Name = item.Name;
					p.AvailableStock = item.AvailableStock;
					p.Cost = item.Cost;
					p.Count = item.Count;
					p.Description = item.Description;
					p.MaxStockThreshold = item.MaxStockThreshold;
					p.PictureFileName = item.PictureFileName;
					p.Price = item.Price;
					p.RestockThreshold = item.RestockThreshold;
					p.SuggestPrice = item.SuggestPrice;
					await context.Products.AddAsync(p);
//					await context.SaveChangesAsync();

					foreach (string category in item.Categories)
					{
						ProductCategory pc = new ProductCategory();
						pc.ProductId = p.Id;
						pc.Category = categories.FirstOrDefault(c => c.Name == category);
						p.ProductCategories.Add(pc);
					}
				}
			}
//			await context.ProductCategories.AddRangeAsync(pcs);
			await context.SaveChangesAsync();
		}

		static private IEnumerable<Category> GetCatalogTypesFromFile(string contentRootPath, ILogger<CatalogContextSeed> logger)
		{
			string fileName = Path.Combine(contentRootPath, "Setup", "CatalogTypes.json");
			if (File.Exists(fileName))
			{
				string raw = File.ReadAllTextAsync(fileName).Result;
				List<Category> data = JsonConvert.DeserializeObject<List<Category>>(raw);
				return data;
			}
			else
			{
				return null;
			}
		}

		async static private Task AddCatalogItemsFromFile(string contentRootPath, CatalogContext context, ILogger<CatalogContextSeed> logger)
		{
			string fileName = Path.Combine(contentRootPath, "Setup", "CatalogItems.json");
			if (File.Exists(fileName))
			{
				string raw = File.ReadAllTextAsync(fileName).Result;
				List<Product> data = JsonConvert.DeserializeObject<List<Product>>(raw);

				foreach (Product item in data)
				{
					Brand br = context.Brands.Where(b => b.Name == item.Brand.Name).FirstOrDefault();
					if (br != null)
					{
						item.BrandId = br.Id;
						item.Brand = br;
					}
				}
				await context.Products.AddRangeAsync(data);
				await context.SaveChangesAsync();

				List<ProductCategory> its = new List<ProductCategory>();
				dynamic items = JArray.Parse(raw);
				foreach (dynamic item in items)
				{
					string name = item.Name;
					int ItemId = data.Find(i => i.Name == name).Id;
					foreach (dynamic input in item.Types2)
					{
						string typeName = input.Name;
						Category type = context.Categories.Where(b => b.Name == typeName).FirstOrDefault();
						if (type != null)
						{
							ProductCategory it = new ProductCategory();
							it.ProductId = ItemId;
							it.CategoryId = type.Id;
							its.Add(it);
						}
					}

				}

				await context.ProductCategories.AddRangeAsync(its);
				await context.SaveChangesAsync();
			}
		}

		//private IEnumerable<Brand> GetCatalogBrandsFromFile(string contentRootPath, ILogger<CatalogContextSeed> logger)
		//      {
		//          string csvFileCatalogBrands = Path.Combine(contentRootPath, "Setup", "CatalogBrands.json");
		//	return JsonConvert.DeserializeObject<List<Brand>>(File.ReadAllText(csvFileCatalogBrands));
		//      }


		//     private IEnumerable<Type> GetCatalogTypesFromFile(string contentRootPath, ILogger<CatalogContextSeed> logger)
		//     {
		//         string csvFileCatalogTypes = Path.Combine(contentRootPath, "Setup", "CatalogTypes.json");
		//if (File.Exists(csvFileCatalogTypes))
		//{
		//	string data = File.ReadAllText(csvFileCatalogTypes);
		//	return JsonConvert.DeserializeObject<List<Type>>(data);
		//}
		//else
		//{
		//	return null;
		//}
		//     }

		//     private IEnumerable<Item> GetCatalogItemsFromFile(string contentRootPath, CatalogContext context, ILogger<CatalogContextSeed> logger)
		//     {
		//         string csvFileCatalogItems = Path.Combine(contentRootPath, "Setup", "CatalogItems.json");
		//return JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText(csvFileCatalogItems));
		//     }

		private void GetCatalogItemPictures(string contentRootPath, string picturePath)
		{
			if (!string.IsNullOrEmpty(contentRootPath) && !string.IsNullOrEmpty(picturePath))
			{
				string zipFileCatalogItemPictures = Path.Combine(contentRootPath, "Setup", "CatalogItems.zip");
				
				if (File.Exists(zipFileCatalogItemPictures))
				{
					if (!Directory.Exists(picturePath))
					{
						Directory.CreateDirectory(picturePath);
					}
					DirectoryInfo directory = new DirectoryInfo(picturePath);
					foreach (FileInfo file in directory.GetFiles())
					{
						file.Delete();
					}
					ZipFile.ExtractToDirectory(zipFileCatalogItemPictures, picturePath);
				}
			}

		}

		private Policy CreatePolicy(ILogger<CatalogContextSeed> logger, string prefix, int retries = 3)
		{
			return Policy.Handle<SqlException>().
				WaitAndRetryAsync(
					retryCount: retries,
					sleepDurationProvider: retry => System.TimeSpan.FromSeconds(5),
					onRetry: (exception, timeSpan, retry, ctx) =>
					{
						logger.LogTrace($"[{prefix}] Exception {exception.GetType().Name} with message ${exception.Message} detected on attempt {retry} of {retries}");
					}
				);
		}
	}
}
