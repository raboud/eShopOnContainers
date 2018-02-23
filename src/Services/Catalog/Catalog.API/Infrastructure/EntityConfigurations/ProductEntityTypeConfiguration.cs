using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HMS.Catalog.API.Model;

namespace HMS.Catalog.API.Infrastructure.EntityConfigurations
{
    class ProductEntityTypeConfiguration
        : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Product");

            builder.Property(ci => ci.Id)
                .ForSqlServerUseSequenceHiLo("catalog_product_hilo")
                .IsRequired();

            builder.Property(ci => ci.Name)
                .IsRequired(true)
                .HasMaxLength(50);

            builder.Property(ci => ci.Price)
                .IsRequired(true);

            builder.Property(ci => ci.PictureFileName)
                .IsRequired(false);

            builder.HasOne(ci => ci.Brand)
                .WithMany()
                .HasForeignKey(ci => ci.BrandId);

			builder.HasOne(ci => ci.Vendor)
				.WithMany()
				.HasForeignKey(ci => ci.VendorId);

			builder.HasOne(ci => ci.Unit)
				.WithMany()
				.HasForeignKey(ci => ci.UnitId);

			builder.HasIndex(p => new { p.Name, p.Count, p.UnitId })
				.IsUnique();

			//            builder.HasOne(ci => ci.CatalogTypes)
			//                .WithMany()
			//                .HasForeignKey(ci => ci.CatalogTypeId);
		}
	}
}
