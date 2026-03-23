namespace PhotoSearch.Models.Image
{
	public class ImageEntry
	{
		public string ImageName { get; set; } = "";

		public string ImagePath { get; set; } = "";

		public float[] Embedding { get; set; } = [];
	}
}
