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
	public class UnitsController : Controller
    {
        private readonly CatalogContext _context;
		private readonly IMapper _mapper;

		public UnitsController(CatalogContext context,
			IMapper mapper)
        {
            _context = context;
			_mapper = mapper;
		}

		// GET: api/Units
		[HttpGet]
		[ProducesResponseType(typeof(List<UnitDTO>), (int)HttpStatusCode.OK)]
		[ResponseCache(Duration = 3600)]
		public async Task<IActionResult> GetItems()
        {
			List<Unit> items = await _context.Units
				.Where(b => !b.InActive)
				.OrderBy(b => b.Name)
				.ToListAsync();

			return Ok(this._mapper.Map<List<UnitDTO>>(items));
		}

		// GET: api/Units
		[HttpGet]
		[Route("[action]")]
		[ProducesResponseType(typeof(PaginatedItemsViewModel<Unit>), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> Page([FromQuery]bool? all, [FromQuery]int pageSize = 10, [FromQuery]int pageIndex = 0)
		{
			IQueryable<Unit> query = _context.Units;
			if (!all.HasValue || all.Value == false)
			{
				query = query
				.Where(b => !b.InActive);
			}

			long totalItems = await query
				.LongCountAsync();

			List<Unit> items = await query
				.OrderBy(b => b.Name)
				.Skip(pageSize * pageIndex)
				.Take(pageSize)
				.ToListAsync();

			PaginatedItemsViewModel<UnitDTO> model = new PaginatedItemsViewModel<UnitDTO>(
				pageIndex, pageSize, totalItems, this._mapper.Map<List<UnitDTO>>(items));

			return Ok(model);
		}

		// GET: api/Units/5
		[HttpGet("{id}")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(UnitDTO), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetUnit([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

			var item = await _context.Units.SingleOrDefaultAsync(m => m.Id == id);
			if (item == null)
			{
				return NotFound();
			}

			return Ok(this._mapper.Map<UnitDTO>(item));
		}

		// PUT: api/Units/5
		[HttpPut("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PutUnit([FromRoute] int id, [FromBody] UnitDTO item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != item.Id)
            {
                return BadRequest();
            }

			_context.Entry(this._mapper.Map<Unit>(item)).State = EntityState.Modified;
			try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UnitExists(id))
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

        // POST: api/Units
        [HttpPost]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PostUnit([FromBody] UnitDTO unit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Units.Add(this._mapper.Map<Unit>(unit));
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUnit", new { id = unit.Id }, unit);
        }

        // DELETE: api/Units/5
        [HttpDelete("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		public async Task<IActionResult> DeleteUnit([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

			var item = await _context.Units.SingleOrDefaultAsync(m => m.Id == id);
			if (item == null)
			{
				return NotFound();
			}

			if (await _context.Products.AnyAsync(p => p.UnitId == id))
			{
				item.InActive = true;
			}
			else
			{
				_context.Units.Remove(item);
			}
			await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UnitExists(int id)
        {
            return _context.Units.Any(e => e.Id == id);
        }
    }
}