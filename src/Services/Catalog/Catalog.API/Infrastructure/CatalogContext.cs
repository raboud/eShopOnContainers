namespace HMS.Catalog.API.Infrastructure
{
    using Microsoft.EntityFrameworkCore;
    using EntityConfigurations;
    using Model;
    using Microsoft.EntityFrameworkCore.Design;

    public class CatalogContext : DbContext
    {
        public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
        {
        }

		internal DbSet<Brand> Brands { get; set; }
		internal DbSet<Category> Categories { get; set; }
		internal DbSet<Product> Products { get; set; }
		internal DbSet<Unit> Units { get; set; }
		internal DbSet<Vendor> Vendors { get; set; }

		internal DbSet<ProductCategory> ProductCategories { get; set; }

		//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		//{
		//	optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFGetStarted.ConsoleApp.NewDb;Trusted_Connection=True;");
		//}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<ProductCategory>()
				.HasKey(x => new { x.ProductId, x.CategoryId });

			modelBuilder.ApplyConfiguration(new BrandEntityTypeConfiguration());
			modelBuilder.ApplyConfiguration(new CategoryEntityTypeConfiguration());
			modelBuilder.ApplyConfiguration(new ProductEntityTypeConfiguration());
		}
    }


    public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
    {
        public CatalogContext CreateDbContext(string[] args)
        {
            var optionsBuilder =  new DbContextOptionsBuilder<CatalogContext>()
//                .UseSqlServer("Server=.;Initial Catalog=HMS.CatalogDb;Integrated Security=true");
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Initial Catalog=HMS.CatalogDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

			return new CatalogContext(optionsBuilder.Options);
        }
    }
}
