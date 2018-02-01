using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.Catalog.API.Model
{
	public class ProductCategory
	{
		public int ItemId { get; set; }
		public Product Item {get;set;}

		public int CategoryId { get; set; }
		public Category Category { get; set; }
    }
}
