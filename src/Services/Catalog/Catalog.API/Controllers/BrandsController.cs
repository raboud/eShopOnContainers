using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HMS.Catalog.API.Infrastructure;
using HMS.Catalog.API.Model;
using System.Net;
using HMS.Catalog.API.ViewModel;

namespace HMS.Catalog.API.Controllers
{
	[Produces("application/json")]
	[Route("api/v1/[controller]")]
	public class BrandsController : Controller
	{
		private readonly CatalogContext _context;

		public BrandsController(CatalogContext context)
		{
			_context = context;
		}

		// GET: api/v1/[controller]
		[HttpGet]
		[ProducesResponseType(typeof(List<Brand>), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetItems([FromQuery]bool? all)
		{
			IQueryable<Brand> query;
			if (all.HasValue && all.Value == true)
			{
				query = _context.Brands;
			}
			else
			{
				query = _context.Brands
				.Where(b => !b.InActive);
			}
			return Ok(await query
				.OrderBy(b => b.Name)
				.ToListAsync()
				);
		}

		// GET: api/v1/[controller]/Page
		[HttpGet]
		[Route("[action]")]
		[ProducesResponseType(typeof(PaginatedItemsViewModel<Brand>), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> Page([FromQuery]bool? all, [FromQuery]int pageSize = 10, [FromQuery]int pageIndex = 0)
		{
			IQueryable<Brand> query = _context.Brands;
			if (!all.HasValue || all.Value == false)
			{
				query = query
				.Where(b => !b.InActive);
			}

			long totalItems = await query
				.LongCountAsync();

			List<Brand> items = await query
				.OrderBy(b => b.Name)
				.Skip(pageSize * pageIndex)
				.Take(pageSize)
				.ToListAsync();

			PaginatedItemsViewModel<Brand> model = new PaginatedItemsViewModel<Brand>(
				pageIndex, pageSize, totalItems, items);

			return Ok(model);
		}

		// GET: api/v1/[controller]/5
		[HttpGet("{id}")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(Brand), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetBrand([FromRoute] int id)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			Brand brand = await _context.Brands.SingleOrDefaultAsync(m => m.Id == id);

			if (brand == null)
			{
				return NotFound();
			}

			return Ok(brand);
		}

		// PUT: api/v1/[controller]/5
		[HttpPut("{id}")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PutBrand([FromRoute] int id, [FromBody] Brand brand)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (id != brand.Id)
			{
				return BadRequest();
			}

			_context.Entry(brand).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!BrandExists(id))
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

		// POST: api/v1/[controller]
		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PostBrand([FromBody] Brand brand)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			_context.Brands.Add(brand);
			await _context.SaveChangesAsync();

			return CreatedAtAction("GetBrand", new { id = brand.Id }, brand);
		}

		// DELETE: api/Brands/5
		[HttpDelete("{id}")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		public async Task<IActionResult> DeleteBrand([FromRoute] int id)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}


			var item = await _context.Brands.SingleOrDefaultAsync(m => m.Id == id);
			if (item == null)
			{
				return NotFound();
			}

			if (await _context.Products.AnyAsync(p => p.BrandId == id))
			{
				item.InActive = true;
			}
			else
			{
				_context.Brands.Remove(item);
			}
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool BrandExists(int id)
		{
			return _context.Brands.Any(e => e.Id == id);
		}
	}
}