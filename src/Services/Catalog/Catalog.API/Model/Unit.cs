using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Catalog.API.Model
{
	internal class Unit
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string Name { get; set; }

		public bool InActive { get; set; }
	}
}

