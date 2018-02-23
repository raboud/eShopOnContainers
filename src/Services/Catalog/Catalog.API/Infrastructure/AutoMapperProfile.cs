using AutoMapper;
using HMS.Catalog.API.Model;
using HMS.Catalog.DTO;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Catalog.API.Infrastructure
{
    public class AutoMapperProfile: Profile
    {
		public AutoMapperProfile()
		{
			CreateMap<Brand, BrandDTO>().ReverseMap();
			CreateMap<Category, CategoryDTO>().ReverseMap();
			CreateMap<Product, ProductDTO>()
				.ForMember(p => p.Brand, opt => opt.MapFrom(src => src.Brand.Name))
				.ForMember(p => p.Unit, opt => opt.MapFrom(src => src.Unit.Name))
				.ForMember(p => p.Vendor, opt => opt.MapFrom(src => src.Vendor.Name))
				.ForMember(p => p.PictureUri, opt => opt.ResolveUsing<PictureUriReslover>())
				.ForMember(p => p.Types, opt => opt.MapFrom(src => src.ProductCategories.Select(e => e.Category.Name).ToList()));
			CreateMap<ProductDTO, Product>()
				.ForMember(p => p.Unit, opt => opt.ResolveUsing<UnitReslover>())
				.ForMember(p => p.Vendor, opt => opt.ResolveUsing<VendorReslover>())
				.ForMember(p => p.Brand, opt => opt.ResolveUsing<BrandReslover>());
			CreateMap<Unit, UnitDTO>().ReverseMap();
			CreateMap<Vendor, VendorDTO>().ReverseMap();
		}
	}

	internal class PictureUriReslover : IValueResolver<Product, ProductDTO, string>
	{
		private readonly CatalogSettings _settings;

		public PictureUriReslover(IOptionsSnapshot<CatalogSettings> settings)
		{
			_settings = settings.Value;
		}

		public string Resolve(Product source, ProductDTO destination, string destMember, ResolutionContext context)
		{
			var baseUri = _settings.PicBaseUrl;

			return _settings.AzureStorageEnabled
				? baseUri + source.PictureFileName
				: baseUri.Replace("[0]", source.Id.ToString());
		}
	}

	internal class UnitReslover : IValueResolver<ProductDTO, Product, Unit>
	{
		private readonly CatalogContext _context;

		public UnitReslover(CatalogContext context)
		{
			_context = context;
		}

		public Unit Resolve(ProductDTO source, Product destination, Unit destMember, ResolutionContext context)
		{
			var item = this._context.Units.FirstOrDefault(u => u.Name == source.Unit);
			destination.UnitId = item.Id;
			return item;
		}
	}

	internal class VendorReslover : IValueResolver<ProductDTO, Product, Vendor>
	{
		private readonly CatalogContext _context;

		public VendorReslover(CatalogContext context)
		{
			_context = context;
		}

		public Vendor Resolve(ProductDTO source, Product destination, Vendor destMember, ResolutionContext context)
		{
			var item = this._context.Vendors.FirstOrDefault(u => u.Name == source.Vendor);
			destination.VendorId = item.Id;
			return item;
		}
	}


	internal class BrandReslover : IValueResolver<ProductDTO, Product, Brand>
	{
		private readonly CatalogContext _context;

		public BrandReslover(CatalogContext context)
		{
			_context = context;
		}

		public Brand Resolve(ProductDTO source, Product destination, Brand destMember, ResolutionContext context)
		{
			var item = this._context.Brands.FirstOrDefault(u => u.Name == source.Brand);
			destination.BrandId = item.Id;
			return item;
		}
	}
}
