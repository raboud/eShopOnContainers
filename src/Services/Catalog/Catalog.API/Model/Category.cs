using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace HMS.Catalog.API.Model
{
	internal class Category
    {
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

        public string Name { get; set; }

		public List<ProductCategory> ProductCategories { get; set; }

		public bool InActive { get; set; }
	}
}
