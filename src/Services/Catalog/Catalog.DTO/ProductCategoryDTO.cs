namespace HMS.Catalog.DTO
{
	public class ProductCategoryDTO
	{
		public int ItemId { get; set; }
		public ProductDTO Item {get;set;}

		public int CategoryId { get; set; }
		public CategoryDTO Category { get; set; }
    }
}
