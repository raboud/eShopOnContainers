using HMS.Catalog.API;
using HMS.Catalog.API.Infrastructure;
using HMS.Catalog.API.IntegrationEvents;
using HMS.Catalog.API.IntegrationEvents.Events;
using HMS.Catalog.API.Model;
using HMS.Catalog.API.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HMS.Catalog.API.Controllers
{
	[Produces("application/json")]
	[Route("api/v1/[controller]")]
	public class ProductsController : Controller
    {
        private readonly CatalogContext _context;
		private readonly CatalogSettings _settings;
		private readonly ICatalogIntegrationEventService _catalogIntegrationEventService;

		public ProductsController(CatalogContext context, IOptionsSnapshot<CatalogSettings> settings, ICatalogIntegrationEventService catalogIntegrationEventService)
        {
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_catalogIntegrationEventService = catalogIntegrationEventService ?? throw new ArgumentNullException(nameof(catalogIntegrationEventService));

			_settings = settings.Value;
			((DbContext)context).ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
		}

		// GET: api/Products
		[HttpGet]
		[ProducesResponseType(typeof(List<Product>), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetItems(
			[FromQuery]string name,
			[FromQuery]int? typeId,
			[FromQuery]int? brandId
		)
        {
			var root = (IQueryable<Product>)_context.Products;

			if (!string.IsNullOrEmpty(name))
			{
				root = root.Where(p => p.Name.StartsWith(name));
			}

			if (typeId.HasValue)
			{
				root = root.Where(ci => ci.ProductCategories.Any(ct => ct.CategoryId == typeId));
			}

			if (brandId.HasValue)
			{
				root = root.Where(ci => ci.BrandId == brandId);
			}

			List<Product> items = await root
				.Where(i => !i.InActive)
				.Include(i => i.Brand)
				.Include(i => i.ProductCategories)
				.ThenInclude(t => t.Category)
				.Include(i => i.Vendor)
				.Include(i => i.Unit)
				.OrderBy(c => c.Name)
				.ToListAsync();
			ChangeUriPlaceholder(items);

			return Ok(items);
		}

		// GET: api/v1/[controller]/Page
		[HttpGet]
		[Route("[action]")]
		[ProducesResponseType(typeof(PaginatedItemsViewModel<Product>), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> Page(
			[FromQuery]string name,
			[FromQuery]int? typeId,
			[FromQuery]int? brandId, 
			[FromQuery]int pageSize = 10, 
			[FromQuery]int pageIndex = 0)
		{
			var root = (IQueryable<Product>)_context.Products;

			if (!string.IsNullOrEmpty(name))
			{
				root = root.Where(p => p.Name.StartsWith(name));
			}

			if (typeId.HasValue)
			{
				root = root.Where(ci => ci.ProductCategories.Any(ct => ct.CategoryId == typeId));
			}

			if (brandId.HasValue)
			{
				root = root.Where(ci => ci.BrandId == brandId);
			}

			long totalItems = await root
				.LongCountAsync();

			List<Product> items = await root
				.Where(i => !i.InActive)
				.Include(i => i.Brand)
				.Include(i => i.ProductCategories)
				.ThenInclude(t => t.Category)
				.Include(i => i.Vendor)
				.Include(i => i.Unit)
				.OrderBy(c => c.Name)
				.Skip(pageSize * pageIndex)
				.Take(pageSize)
				.ToListAsync();

			ChangeUriPlaceholder(items);

			PaginatedItemsViewModel<Product> model = new PaginatedItemsViewModel<Product>(
				pageIndex, pageSize, totalItems, items);

			return Ok(model);
		}

		private void ChangeUriPlaceholder(List<Product> items)
		{
			var baseUri = _settings.PicBaseUrl;

			items.ForEach(catalogItem =>
			{
				ChangeUriPlaceholder(catalogItem);
			});
		}

		private void ChangeUriPlaceholder(Product item)
		{
			var baseUri = _settings.PicBaseUrl;

				item.PictureUri = _settings.AzureStorageEnabled
					? baseUri + item.PictureFileName
					: baseUri.Replace("[0]", item.Id.ToString());
		}


		// GET: api/Products/5
		[HttpGet("{id}")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

			if (id <= 0)
			{
				return BadRequest();
			}

			var product = await _context.Products
				.Include(i => i.Brand)
				.Include(i => i.ProductCategories)
				.ThenInclude(t => t.Category)
				.Include(i => i.Vendor)
				.Include(i => i.Unit)
				.SingleOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }
			ChangeUriPlaceholder(product);

			return Ok(product);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PutProduct([FromRoute] int id, [FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.Id)
            {
                return BadRequest();
            }

			Product p = await _context.Products
				.SingleOrDefaultAsync(i => i.Id == product.Id);

			if (p == null)
			{
				return NotFound(new { Message = $"Item with id {product.Id} not found." });
			}

			bool raiseProductPriceChangedEvent = p.Price != product.Price;

			try
			{
				if (product.Types2 != null)
				{
					List<ProductCategory> toAdd = new List<ProductCategory>();
					List<ProductCategory> toDel = new List<ProductCategory>();
					List<Category> types = await _context.Categories.ToListAsync();
					List<ProductCategory> current = await _context
						.ProductCategories
						.Where(pc => pc.ItemId == id)
						.Include(pc => pc.Category)
						.ToListAsync();

					foreach (ProductCategory pc in current)
					{
						if (!product.Types2.Contains(pc.Category.Name))
						{
							toDel.Add(pc);
						}
						else
						{
							product.Types2.Remove(pc.Category.Name);
						}
					}

					foreach (string type in product.Types2)
					{
						ProductCategory pc = new ProductCategory()
						{
							ItemId = product.Id,
							CategoryId = types.Where(t => t.Name == type).SingleOrDefault().Id
						};
						toAdd.Add(pc);
					}
					await _context.AddRangeAsync(toAdd);
					_context.RemoveRange(toDel);
				}

				if (raiseProductPriceChangedEvent) // Save product's data and publish integration event through the Event Bus if price has changed
				{
					//Create Integration Event to be published through the Event Bus
					var priceChangedEvent = new ProductPriceChangedIntegrationEvent(product.Id, product.Price, p.Price);

					// Achieving atomicity between original Catalog database operation and the IntegrationEventLog thanks to a local transaction
					await _catalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync(priceChangedEvent);

					// Publish through the Event Bus and mark the saved event as published
					await _catalogIntegrationEventService.PublishThroughEventBusAsync(priceChangedEvent);
				}

				_context.Entry(product).State = EntityState.Modified;
				await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        [HttpPost]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PostProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

			var item = await _context.Products.SingleOrDefaultAsync(m => m.Id == id);
			if (item == null)
			{
				return NotFound();
			}

			item.InActive = true;
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}