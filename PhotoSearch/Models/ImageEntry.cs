namespace PhotoSearch.Models
{
	public class ImageEntry
	{
		public string ImageName { get; set; } = "";

		public string ImagePath { get; set; } = "";

		public float[] Embedding { get; set; } = [];
	}
}
