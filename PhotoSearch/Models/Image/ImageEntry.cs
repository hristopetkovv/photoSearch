namespace PhotoSearch.Models.Image
{
	public class ImageEntry
	{
		public int Id { get; set; }

		public string ImageName { get; set; } = "";

		public string ImagePath { get; set; } = "";

		public Vector Embedding { get; set; } = null!;
	}
}
