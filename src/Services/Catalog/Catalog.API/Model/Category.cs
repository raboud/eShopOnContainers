using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace HMS.Catalog.API.Model
{
	public class Category
    {
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

        public string Name { get; set; }

		[JsonIgnore]
		public List<ProductCategory> ProductCategories { get; set; }

		[JsonIgnore]
		[NotMapped]
		public IEnumerable<Product> Items => ProductCategories?.Select(e => e.Item);

		public bool InActive { get; set; }
	}
}
