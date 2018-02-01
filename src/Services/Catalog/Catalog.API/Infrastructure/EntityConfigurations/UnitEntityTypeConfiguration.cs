using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HMS.Catalog.API.Model;

namespace HMS.Catalog.API.Infrastructure.EntityConfigurations
{
	class UnitEntityTypeConfiguration
		: IEntityTypeConfiguration<Unit>
	{
		public void Configure(EntityTypeBuilder<Unit> builder)
		{
			builder.ToTable("Unit");

			builder.HasKey(ci => ci.Id);

			builder.Property(ci => ci.Id)
			   .ForSqlServerUseSequenceHiLo("catalog_unit_hilo")
			   .IsRequired();

			builder.Property(cb => cb.Name)
				.IsRequired()
				.HasMaxLength(100);

			builder.HasIndex(v => v.Name)
				.IsUnique();
		}
	}

}
