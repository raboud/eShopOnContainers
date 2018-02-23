using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.Catalog.API.Model
{
	internal class ProductCategory
	{
		public int ProductId { get; set; }
		public Product Product { get;set;}

		public int CategoryId { get; set; }
		public Category Category { get; set; }
    }
}
