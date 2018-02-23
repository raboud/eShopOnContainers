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
using Microsoft.AspNetCore.Authorization;
using HMS.Common.API;
using HMS.Catalog.DTO;
using AutoMapper;

namespace HMS.Catalog.API.Controllers
{
    [Produces("application/json")]
	[Route("api/v1/[controller]")]
	public class CategoriesController : Controller
    {
        private readonly CatalogContext _context;
		private readonly IMapper _mapper;

		public CategoriesController(CatalogContext context,
			IMapper mapper)
        {
            _context = context;
			_mapper = mapper;
		}

		// GET: api/Categories
		[HttpGet]
		[ProducesResponseType(typeof(List<CategoryDTO>), (int)HttpStatusCode.OK)]
		[ResponseCache(Duration = 3600)]
		public async Task<IActionResult> GetCategories()
		{
			List<Category> items = await _context.Categories
				.Where(b => !b.InActive)
				.OrderBy(b => b.Name)
				.ToListAsync();

			return Ok(this._mapper.Map<List<CategoryDTO>>(items));
		}

		// GET: api/v1/[controller]/Page
		[HttpGet]
		[Route("[action]")]
		[ProducesResponseType(typeof(PaginatedItemsViewModel<CategoryDTO>), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> Page([FromQuery]bool? all, [FromQuery]int pageSize = 10, [FromQuery]int pageIndex = 0)
		{
			IQueryable<Category> query = _context.Categories;

			if (!all.HasValue || all.Value == false)
			{
				query = query
				.Where(b => !b.InActive);
			}

			long totalItems = await query
				.LongCountAsync();

			List<Category> items = await query
				.OrderBy(b => b.Name)
				.Skip(pageSize * pageIndex)
				.Take(pageSize)
				.ToListAsync();

			PaginatedItemsViewModel<CategoryDTO> model = new PaginatedItemsViewModel<CategoryDTO>(
				pageIndex, pageSize, totalItems, this._mapper.Map<List<CategoryDTO>>(items));

			return Ok(model);
		}

		// GET: api/Categories/5
		[HttpGet("{id}")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(CategoryDTO), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetCategory([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _context.Categories.SingleOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(this._mapper.Map<List<CategoryDTO>>(category));
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PutCategory([FromRoute] int id, [FromBody] CategoryDTO item)
        {
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (id != item.Id)
			{
				return BadRequest();
			}

			_context.Entry(this._mapper.Map<Category>(item)).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!Exists(id))
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

		// POST: api/Categories
		[HttpPost]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PostCategory([FromBody] CategoryDTO category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Categories.Add(this._mapper.Map<Category>(category));
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		public async Task<IActionResult> DeleteCategory([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var item = await _context.Categories.SingleOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

			if (await _context.ProductCategories.AnyAsync(pc => pc.CategoryId == id))
			{
				item.InActive = true;
			}
			else
			{
				_context.Categories.Remove(item);
			}
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool Exists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}