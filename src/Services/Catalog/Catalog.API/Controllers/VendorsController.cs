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
	public class VendorsController : Controller
    {
        private readonly CatalogContext _context;
		private readonly IMapper _mapper;

		public VendorsController(CatalogContext context,
			IMapper mapper)
        {
            _context = context;
			_mapper = mapper;
		}

		// GET: api/Vendors
		[HttpGet]
		[ResponseCache(Duration = 3600)]
		[ProducesResponseType(typeof(List<VendorDTO>), (int)HttpStatusCode.OK)]
		public async  Task<IActionResult> GetVendors()
        {
			List<Vendor> items = await _context.Vendors
				.Where(b => !b.InActive)
				.OrderBy(b => b.Name)
				.ToListAsync();

			return Ok(this._mapper.Map<List<VendorDTO>>(items));
		}

		// GET: api/v1/[controller]/Page
		[HttpGet]
		[Route("[action]")]
		[ProducesResponseType(typeof(PaginatedItemsViewModel<VendorDTO>), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> Page([FromQuery]bool? all, [FromQuery]int pageSize = 10, [FromQuery]int pageIndex = 0)
		{
			IQueryable<Vendor> query = _context.Vendors;
			if (!all.HasValue || all.Value == false)
			{
				query = query
				.Where(b => !b.InActive);
			}

			long totalItems = await query
				.LongCountAsync();

			List<Vendor> items = await query
				.OrderBy(b => b.Name)
				.Skip(pageSize * pageIndex)
				.Take(pageSize)
				.ToListAsync();

			PaginatedItemsViewModel<VendorDTO> model = new PaginatedItemsViewModel<VendorDTO>(
				pageIndex, pageSize, totalItems, this._mapper.Map<List<VendorDTO>>(items));

			return Ok(model);
		}

		// GET: api/Vendors/5
		[HttpGet("{id}")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType(typeof(Vendor), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetVendor([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vendor = await _context.Vendors.SingleOrDefaultAsync(m => m.Id == id);

            if (vendor == null)
            {
                return NotFound();
            }

            return Ok(this._mapper.Map<VendorDTO>(vendor));
        }

        // PUT: api/Vendors/5
        [HttpPut("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PutVendor([FromRoute] int id, [FromBody] VendorDTO vendor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != vendor.Id)
            {
                return BadRequest();
            }

            _context.Entry(this._mapper.Map<Vendor>(vendor)).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VendorExists(id))
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

        // POST: api/Vendors
        [HttpPost]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.Created)]
		public async Task<IActionResult> PostVendor([FromBody] VendorDTO vendor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Vendors.Add(this._mapper.Map<Vendor>(vendor));
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVendor", new { id = vendor.Id }, vendor);
        }

        // DELETE: api/Vendors/5
        [HttpDelete("{id}")]
		[Authorize(Roles = "admin")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		public async Task<IActionResult> DeleteVendor([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

			var item = await _context.Vendors.SingleOrDefaultAsync(m => m.Id == id);
			if (item == null)
			{
				return NotFound();
			}

			if (await _context.Products.AnyAsync(p => p.VendorId == id))
			{
				item.InActive = true;
			}
			else
			{
				_context.Vendors.Remove(item);
			}
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool VendorExists(int id)
        {
            return _context.Vendors.Any(e => e.Id == id);
        }
    }
}