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

		public DbSet<Brand> Brands { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<Unit> Units { get; set; }
		public DbSet<Vendor> Vendors { get; set; }

		public DbSet<ProductCategory> ProductCategories { get; set; }

		//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		//{
		//	optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFGetStarted.ConsoleApp.NewDb;Trusted_Connection=True;");
		//}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<ProductCategory>()
				.HasKey(x => new { x.ItemId, x.CategoryId });

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
