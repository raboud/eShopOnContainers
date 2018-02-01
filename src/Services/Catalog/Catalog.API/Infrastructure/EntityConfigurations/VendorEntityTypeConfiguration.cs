using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HMS.Catalog.API.Model;

namespace HMS.Catalog.API.Infrastructure.EntityConfigurations
{
	class VendorEntityTypeConfiguration
		: IEntityTypeConfiguration<Vendor>
	{
		public void Configure(EntityTypeBuilder<Vendor> builder)
		{
			builder.ToTable("Vendor");

			builder.HasKey(ci => ci.Id);

			builder.Property(ci => ci.Id)
			   .ForSqlServerUseSequenceHiLo("catalog_vendor_hilo")
			   .IsRequired();

			builder.Property(cb => cb.Name)
				.IsRequired()
				.HasMaxLength(100);

			builder.HasIndex(v => v.Name)
				.IsUnique();
		}
	}

}
